using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Security.Cryptography;

namespace Basic_Crud.Services
{
    public class UtilityService
    {
        private readonly IHttpContextAccessor httpContext;

        public UtilityService(IHttpContextAccessor httpContext)
        {
            this.httpContext = httpContext;
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
    }
}
