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

        public async Task<(User?, bool, bool)> GetUserDetails()
        {
            (User? user, bool loggedIn, bool userExist) = await utilityService.GetLoggedInUserDetails();

            return (user, loggedIn, userExist);
        }

        public async Task<(User?, bool, bool)> UpdateUser(UserUpdateDto userUpdateDto)
        {
            (User? user, bool loggedIn, bool userExist) = await utilityService.GetLoggedInUserDetails();

            if (!loggedIn || !userExist || user == null) return (user, loggedIn, userExist);

            if (userUpdateDto.Username != null) user.Username = userUpdateDto.Username;

            if (userUpdateDto.Email != null) user.Email = userUpdateDto.Email;

            context.Users.Update(user);
            context.SaveChanges();

            return (user, loggedIn, userExist);
        }

        public async Task<(User?, bool, bool)> UpdatePassword(UserUpdatePassowrd updatePassowrd)
        {
            (User? user, bool loggedIn, bool userExist) = await utilityService.GetLoggedInUserDetails();

            if (user == null) return (user, loggedIn, userExist);

            utilityService.CreatePassword(updatePassowrd.Password, out byte[] salt, out byte[] hash);

            user.PasswordSalt = salt;
            user.PasswordHash = hash;

            context.Users.Update(user);
            context.SaveChanges();

            return (user, loggedIn, userExist);
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

        public async Task<(List<Item>, bool, bool)> GetUserItems()
        {
            (User? user, bool loggedIn, bool userExist) = await utilityService.GetLoggedInUserDetails();
            if (!loggedIn || !userExist || user == null) return (new List<Item>(), loggedIn, userExist);

            var items = await context.Items.Where(i => i.Owner == user).ToListAsync();

            return (items, loggedIn, userExist);
        }

        public async Task<List<Auction>> GetUserAuctions()
        {
            (User? user, bool loggedIn, bool userExist) = await utilityService.GetLoggedInUserDetails();

            return await context.Auctions.Where(a => a.Item.OwnerId == user.Id).ToListAsync();
        }
    }
}
