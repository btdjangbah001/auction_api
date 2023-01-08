using Basic_Crud.Data;
using Basic_Crud.Models;
using Microsoft.EntityFrameworkCore;

namespace Basic_Crud.Services
{
    public class ItemsService
    {
        private readonly AppDBContext context;
        private readonly IConfiguration configuration;

        public ItemsService(AppDBContext context, IConfiguration configuration)
        {
            this.context = context;
            this.configuration = configuration;
        }

        public async Task<List<Item>> GetAll()
        {
            var items = context.Items.Select(q => new Item
            {
                Id = q.Id,
                Owner = q.Owner,
                Category = q.Category,
                Description = q.Description,
                Sold = q.Sold,
                Auctions = q.Auctions,
            });

            return await items.ToListAsync();
        }

        public async Task<Item?> GetItem(int id)
        {
            return await context.Items.FindAsync(id);
        }

        public async Task<Tuple<Item?, bool, bool>?> CreateItem(ItemDto itemDto, string? loggedInUsername)
        {
            if (loggedInUsername == null) return null;

            var user = await context.Users.Where(u => u.Username == loggedInUsername).FirstOrDefaultAsync();

            var userExists = true;

            if (user == null)
            {
                userExists = false;
            }

            var category = await context.Categories.Where(c => c.Name == itemDto.Category).FirstOrDefaultAsync();

            var categoryExists = true;

            if (category == null)
            {
                categoryExists = false;
            }


            Item? item = null;

            if (userExists && categoryExists)
            {
                item = new Item
                {
                    Name = itemDto.Name,
                    Owner = user,
                    Category = category,
                    Description = itemDto.Description,
                    Created = Timesta,
                };

                await context.Items.AddAsync(item);
                await context.SaveChangesAsync();
            }

            return new Tuple<Item?, bool, bool>(item, userExists, categoryExists);
        }
    }
}
