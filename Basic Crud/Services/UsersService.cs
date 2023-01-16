using Basic_Crud.Data;
using Basic_Crud.Models;
using Microsoft.EntityFrameworkCore;

namespace Basic_Crud.Services
{
    public class UsersService
    {
        private readonly AppDBContext context;
        private readonly UtilityService utilityService;

        public UsersService(AppDBContext context, UtilityService utilityService)
        {
            this.context = context;
            this.utilityService = utilityService;
        }

        public async Task<Tuple<User?, bool>> GetUserDetails()
        {
            string? username = null;
            bool userFound = true;
            User? user = null;

            username = utilityService.GetLoggedInUser();

            if (string.IsNullOrWhiteSpace(username)) userFound = false;
            else user = await context.Users.Where(u => u.Username == username).FirstOrDefaultAsync();

            return new Tuple<User?, bool>(user, userFound);
        }

        public async Task<Tuple<User?, bool>> UpdateUser(UserUpdateDto userUpdateDto)
        {
            string? loggedInUser = null;
            bool userFound = true;
            User? user = null;

            loggedInUser = utilityService.GetLoggedInUser();

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

            string? username = utilityService.GetLoggedInUser();

            if (username == null) return new Tuple<User?, bool>(user, loggedInUser);

            user = await context.Users.Where(u => u.Username == username).FirstOrDefaultAsync();

            if (user == null) return new Tuple<User?, bool>(user, loggedInUser);

            utilityService.CreatePassword(updatePassowrd.Password, out byte[] salt, out byte[] hash);

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

        public async Task<Tuple<List<Item>?, bool, bool>> GetUserItems()
        {
            var userLoggedIn = true;
            var userExists = true;

            var username = utilityService.GetLoggedInUser();
            if (username == null)
            {
                userLoggedIn = false;
                return new Tuple<List<Item>?, bool, bool>(null, userLoggedIn, false);
            }

            var user = await context.Users.Where(u => u.Username == username).FirstOrDefaultAsync();
            if (user == null)
            {
                userExists = false;
                return new Tuple<List<Item>?, bool, bool>(null, userLoggedIn, userExists);
            }

            var item = await context.Items.Where(i => i.Owner == user).ToListAsync();

            if (item == null) item = new List<Item> { };

            return new Tuple<List<Item>?, bool, bool>(item, userLoggedIn, userExists);
        }
    }
}
