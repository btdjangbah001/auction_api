using Basic_Crud.Data;
using Basic_Crud.Models;
using Basic_Crud.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Basic_Crud.Services
{
    public class UsersService
    {
        private readonly AppDBContext context;
        private readonly UserRepository userRepo;
        private readonly UtilityService utilityService;

        public UsersService(AppDBContext context, UserRepository userRepo, UtilityService utilityService)
        {
            this.context = context;
            this.userRepo = userRepo;
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

            userRepo.UpdateUser(user);

            return (user, loggedIn, userExist);
        }

        public async Task<(User?, bool, bool)> UpdatePassword(UserUpdatePassowrd updatePassowrd)
        {
            (User? user, bool loggedIn, bool userExist) = await utilityService.GetLoggedInUserDetails();

            if (user == null) return (user, loggedIn, userExist);

            utilityService.CreatePassword(updatePassowrd.Password, out byte[] salt, out byte[] hash);

            user.PasswordSalt = salt;
            user.PasswordHash = hash;

            userRepo.UpdateUser(user);

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
            var user = await userRepo.FindOneByUsername(username);

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
