﻿using Basic_Crud.Data;
using Basic_Crud.Models;
using System.Security.Cryptography;
using System.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace Basic_Crud.Services
{
    public class AuthService
    {
        private readonly AppDBContext context;
        private readonly IConfiguration configuration;

        public AuthService(AppDBContext context, IConfiguration configuration)
        {
            this.context = context;
            this.configuration = configuration;
        }

        public async Task<User?> RegisterUser(UserDto userReq)
        {
            var existingUser = await context.Users.Where(u => u.Username == userReq.Username).FirstOrDefaultAsync();

            if (existingUser != null)
                return null;

            var user = new User();
            
            CreatePassword(userReq.Password, out byte[] salt, out byte[] hash);

            user.Username = userReq.Username;
            user.Email = userReq.Email;
            user.PasswordSalt = salt;
            user.PasswordHash = hash;

            await context.AddAsync(user);
            await context.SaveChangesAsync();

            return user;
        }

        public async Task<string?> LoginUser(UserLogin userLogin)
        {
            var user = await context.Users.Where(x => x.Username == userLogin.Username).FirstOrDefaultAsync();
            
            if (user == null)
                return null;

            if (!VerifyPassword(userLogin.Password, user))
                return null;

            return CreateToken(user);
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

        public void CreatePassword(string password, out byte[] passwordSalt, out byte[] passwodHash)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwodHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        bool VerifyPassword(string password, User user)
        {
            using(var hmac = new HMACSHA512(user.PasswordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(user.PasswordHash);
            }
        }
    }
}
