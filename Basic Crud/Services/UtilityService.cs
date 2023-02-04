using Basic_Crud.Data;
using Basic_Crud.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;

namespace Basic_Crud.Services
{
    public class UtilityService
    {
        private readonly IHttpContextAccessor httpContext;
        private readonly AppDBContext context;

        public UtilityService(IHttpContextAccessor httpContext, AppDBContext context)
        {
            this.httpContext = httpContext;
            this.context = context;
        }

        public void CreatePassword(string password, out byte[] passwordSalt, out byte[] passwodHash)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwodHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        public string? GetLoggedInUser()
        {
            string? username = null;
            if (httpContext.HttpContext != null) username = httpContext.HttpContext.User.FindFirstValue(ClaimTypes.Name);

            if (string.IsNullOrWhiteSpace(username)) return null;

            else return username;
        }

        public async Task<(User?, bool, bool)> GetLoggedInUserDetails()
        {
            string? loggedInUser = GetLoggedInUser();
            User? user = null;
            bool loggedIn, userExist;
            loggedIn = userExist = false;

            if (loggedInUser == null) return (null, loggedIn, userExist);
            loggedIn = true;

            user = await context.Users.Where(q => q.Username == loggedInUser).FirstOrDefaultAsync();

            if (user == null) return (null, loggedIn, userExist);
            userExist = true;

            return (user, loggedIn, userExist);
        }
    }
}
