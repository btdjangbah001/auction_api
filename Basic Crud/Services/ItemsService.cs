using Basic_Crud.Data;
using Basic_Crud.Models;
using Microsoft.EntityFrameworkCore;

namespace Basic_Crud.Services
{
    public class ItemsService
    {
        private readonly AppDBContext context;
        private readonly UtilityService utilityService;

        public ItemsService(AppDBContext context, UtilityService utilityService)
        {
            this.context = context;
            this.utilityService = utilityService;
        }

        public async Task<List<Item>> GetAll()
        {
            var items = context.Items.Select(q => new Item
            {
                Id = q.Id,
                Name = q.Name,
                Owner = q.Owner,
                Category = q.Category,
                Description = q.Description,
                Created = q.Created,
                Sold = q.Sold,
                Auctions = q.Auctions,
            });

            return await items.ToListAsync();
        }

        public async Task<Item?> GetItem(int id)
        {
            return await context.Items.FindAsync(id);
        }

        public async Task<(Item?, bool, bool, bool)> CreateItem(ItemDto itemDto)
        {
            bool categoryExists = false;

            (User? user, bool loggedIn, bool userExists) = await utilityService.GetLoggedInUserDetails();

            if (!loggedIn || !userExists) return (null, loggedIn, userExists, categoryExists);

            var category = await context.Categories.Where(c => c.Name == itemDto.Category).FirstOrDefaultAsync();

            if (category == null) return (null, loggedIn, userExists, categoryExists);
            categoryExists = true;

            Item? item = new Item
            {
                Name = itemDto.Name,
                Owner = user!,
                Category = category,
                Description = itemDto.Description,
                Created = DateTime.UtcNow,
            };

            await context.Items.AddAsync(item);
            await context.SaveChangesAsync();

            return (item, loggedIn, userExists, categoryExists);
        }
    }
}
