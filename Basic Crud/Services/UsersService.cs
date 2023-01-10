using Basic_Crud.Data;
using Basic_Crud.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Basic_Crud.Services
{
    public class UsersService
    {
        private readonly AppDBContext context;
        private readonly IHttpContextAccessor httpContext;
        private readonly AuthService authService;

        public UsersService(AppDBContext context, IHttpContextAccessor httpContext, AuthService authService)
        {
            this.context = context;
            this.httpContext = httpContext;
            this.authService = authService;
        }

        public async Task<Tuple<User?, bool>> GetUserDetails()
        {
            string? username = null;
            bool userFound = true;
            User? user = null;

            if (httpContext.HttpContext != null) username = httpContext.HttpContext.User.FindFirstValue(ClaimTypes.Name);

            if (string.IsNullOrWhiteSpace(username)) userFound = false;
            else user = await context.Users.Where(u => u.Username == username).FirstOrDefaultAsync();

            return new Tuple<User?, bool>(user, userFound);
        }

        public async Task<Tuple<User?, bool>> UpdateUser(UserUpdateDto userUpdateDto)
        {
            string? loggedInUser = null;
            bool userFound = true;
            User? user = null;

            if (httpContext.HttpContext != null) loggedInUser = httpContext.HttpContext.User.FindFirstValue(ClaimTypes.Name);

            if (string.IsNullOrWhiteSpace(loggedInUser)) userFound = false;
            else user =await context.Users.Where(u => u.Username == loggedInUser).FirstOrDefaultAsync();

            if (user == null) return new Tuple<User?, bool>(user, userFound);

            if (userUpdateDto.Username != null) user.Username = userUpdateDto.Username;

            if (userUpdateDto.Email != null) user.Email = userUpdateDto.Email;

            context.Users.Update(user);
            context.SaveChanges();

            return new Tuple<User?, bool>(user, userFound);
        }

        public async Task<Tuple<User?, bool>> UpdatePassword(UserUpdatePassowrd updatePassowrd)
        {
            if (updatePassowrd.Password != updatePassowrd.ConfirmPassword) return null;

            bool loggedInUser = true;
            User? user = null;

            string? username = GetLoggedInUser();

            if (username == null) return new Tuple<User?, bool>(user, loggedInUser);

            user = await context.Users.Where(u => u.Username == username).FirstOrDefaultAsync();

            if (user == null) return new Tuple<User?, bool>(user, loggedInUser);

            authService.CreatePassword(updatePassowrd.Password, out byte[] salt, out byte[] hash);

            user.PasswordSalt = salt;
            user.PasswordHash = hash;

            context.Users.Update(user);
            context.SaveChanges();

            return new Tuple<User?, bool>(user, loggedInUser);
        }

        public async Task<bool> IsUsernameAvailable(string? username)
        {
            if (username != null)
            {
                var usernameExists = await UsernameExists(username);
                if (usernameExists)
                {
                    return false;
                }
            }
            return true;
        }

        private async Task<bool> UsernameExists(string username)
        {
            var user = await context.Users.Where(u => u.Username == username).FirstOrDefaultAsync();

            return user != null;
        }

        private string? GetLoggedInUser()
        {
            string? username = null;

            if (httpContext.HttpContext != null) username = httpContext.HttpContext.User.FindFirstValue(ClaimTypes.Name);

            if (string.IsNullOrWhiteSpace(username)) return null;

            else return username;
        }
    }
}
