using Basic_Crud.Data;
using Basic_Crud.Models;
using System.Security.Cryptography;
using System.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Http;

namespace Basic_Crud.Services
{
    public class AuthService
    {
        private readonly AppDBContext context;
        private readonly IConfiguration configuration;
        private readonly UtilityService utilityService;

        public AuthService(AppDBContext context, IConfiguration configuration, UtilityService utilityService)
        {
            this.context = context;
            this.configuration = configuration;
            this.utilityService = utilityService;
        }

        public async Task<User?> RegisterUser(UserDto userReq)
        {
            var existingUser = await context.Users.Where(u => u.Username == userReq.Username).FirstOrDefaultAsync();

            if (existingUser != null)
                return null;

            var user = new User();
            
            utilityService.CreatePassword(userReq.Password, out byte[] salt, out byte[] hash);

            user.Username = userReq.Username;
            user.Email = userReq.Email;
            user.PasswordSalt = salt;
            user.PasswordHash = hash;

            await context.AddAsync(user);
            await context.SaveChangesAsync();

            return user;
        }

        public async Task<string?> LoginUser(UserLogin userLogin, HttpResponse response)
        {
            var user = await context.Users.Where(x => x.Username == userLogin.Username).FirstOrDefaultAsync();
            
            if (user == null)
                return null;

            if (!VerifyPassword(userLogin.Password, user))
                return null;

            var refreshToken = GenerateRefreshToken();
            SetRefreshToken(refreshToken, response, user);

            return CreateToken(user);
        }

        public async Task<(User?, bool, bool, bool, bool, string)> RefreshToken(HttpRequest request, HttpResponse response)
        {
            var refreshToken = request.Cookies["refreshToken"];
            bool validToken = false;
            bool tokenNotExpired = false;

            (User? user, bool loggedIn, bool userExists) = await utilityService.GetLoggedInUserDetails();

            if (!loggedIn || !userExists || user == null)
                return (user, loggedIn, userExists, validToken, tokenNotExpired, String.Empty);

            if (!user.RefreshToken.Equals(refreshToken))
                return (user, loggedIn, userExists, validToken, tokenNotExpired, String.Empty);
            validToken = true;

            if (user.TokenExpires < DateTime.UtcNow)
                return (user, loggedIn, userExists, validToken, tokenNotExpired, String.Empty);
            tokenNotExpired = true;

            var token = CreateToken(user);
            var refToken = GenerateRefreshToken();
            SetRefreshToken(refToken, response, user);

            return (user, loggedIn, userExists, validToken, tokenNotExpired, token);
        }

        string CreateToken(User user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username)
            };

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(configuration.GetSection("AppSettings:Token").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        bool VerifyPassword(string password, User user)
        {
            using(var hmac = new HMACSHA512(user.PasswordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(user.PasswordHash);
            }
        }

        private RefreshToken GenerateRefreshToken()
        {
            var refreshToken = new RefreshToken
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                Expires = DateTime.UtcNow.AddDays(7),
                Created = DateTime.UtcNow,
            };

            return refreshToken;
        }

        private void SetRefreshToken(RefreshToken refreshToken, HttpResponse response, User user)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = refreshToken.Expires,
            };

            response.Cookies.Append("refreshToken", refreshToken.Token, cookieOptions);

            user.RefreshToken = refreshToken.Token;
            user.TokenCreated = refreshToken.Created;
            user.TokenExpires = refreshToken.Expires;

            context.Users.Update(user);
            context.SaveChanges();
        }
    }
}
