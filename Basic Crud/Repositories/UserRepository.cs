using Basic_Crud.Data;
using Basic_Crud.Models;
using Microsoft.EntityFrameworkCore;

namespace Basic_Crud.Repositories
{
    public class UserRepository
    {
        private readonly AppDBContext context;

        public UserRepository(AppDBContext context)
        {
            this.context = context;
        }

        public async Task<User?> FindOneByUsername(string username)
        {
            return await context.Users.Where(u => u.Username == username).FirstOrDefaultAsync();
        }

        public async Task SaveUser(User user)
        {
            await context.AddAsync(user);
            await context.SaveChangesAsync();
        }

        public void UpdateUser(User user)
        {
            context.Users.Update(user);
            context.SaveChanges();
        }
    }
}
