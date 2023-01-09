using Basic_Crud.Data;
using Basic_Crud.Models;
using Microsoft.EntityFrameworkCore;

namespace Basic_Crud.Services
{
    public class UsersService
    {
        private readonly AppDBContext context;

        public UsersService(AppDBContext context)
        {
            this.context = context;
        }

        public async Task<User?> GetUserDetails(string? username)
        {
            if (string.IsNullOrWhiteSpace(username)) return null;

            var user = await context.Users.Where(u => u.Username == username).FirstOrDefaultAsync();

            return user;
        }

        public async Task<User?> UpdateUser(UserUpdateDto userUpdateDto, string? loggedInUser)
        {
            var user = context.Users.Where(u => u.Username == loggedInUser).FirstOrDefault();

            if (user == null) return null;

            if (userUpdateDto.Username != null) user.Username = userUpdateDto.Username;
            if (userUpdateDto.Email != null) user.Email = userUpdateDto.Email;

            context.Users.Update(user);
            await context.SaveChangesAsync();

            return user;
        }
    }
}
