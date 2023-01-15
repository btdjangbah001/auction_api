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
        private readonly UsersService usersService;
        private readonly UtilityService utilityService;

        public AuthService(AppDBContext context, IConfiguration configuration, UsersService usersService, UtilityService utilityService)
        {
            this.context = context;
            this.configuration = configuration;
            this.usersService = usersService;
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

        public async Task<Tuple<User?, bool, bool, bool, bool>> RefreshToken(HttpRequest request, HttpResponse response)
        {
            var refreshToken = request.Cookies["refreshToken"];
            var username = utilityService.GetLoggedInUser();
            bool userIsLoggedIn = true;
            bool userExists = true;
            bool validToken = true;
            bool tokenNotExpired = true;

            User? user = null;

            if (string.IsNullOrWhiteSpace(username))
            {
                userIsLoggedIn = false;
                return new Tuple<User?, bool, bool, bool, bool>(null, userIsLoggedIn, false, false, false);
            }
            else user = await context.Users.Where(u => u.Username == username).FirstOrDefaultAsync();

            if (user == null)
            {
                userExists = false;
                return new Tuple<User?, bool, bool, bool, bool>(null, userIsLoggedIn, userExists, false, false);
            }
            if (!user.RefreshToken.Equals(refreshToken))
            {
                validToken = false;
                return new Tuple<User?, bool, bool, bool, bool>(user, userIsLoggedIn, userExists, validToken, false);
            }
            else if (user.TokenExpires < DateTime.UtcNow)
            {
                tokenNotExpired = false;
                return new Tuple<User?, bool, bool, bool, bool>(user, userIsLoggedIn, userExists, validToken, tokenNotExpired);
            }

            var token = CreateToken(user);
            var refToken = GenerateRefreshToken();
            SetRefreshToken(refToken, response, user);

            return new Tuple<User?, bool, bool, bool, bool>(user, userIsLoggedIn, userExists, validToken, tokenNotExpired);
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
                expires: DateTime.Now.AddMinutes(15),
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
