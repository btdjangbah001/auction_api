using Basic_Crud.Data;
using Basic_Crud.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Basic_Crud.Services
{
    public class ItemsService
    {
        private readonly AppDBContext context;
        private readonly IHttpContextAccessor httpContext;
        private readonly UtilityService utilityService;

        public ItemsService(AppDBContext context, IHttpContextAccessor httpContext, UtilityService utilityService)
        {
            this.context = context;
            this.httpContext = httpContext;
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

        public async Task<Tuple<Item?, bool, bool, bool>?> CreateItem(ItemDto itemDto)
        {
            string? loggedInUsername = null;

            if (httpContext.HttpContext != null) loggedInUsername = httpContext.HttpContext.User.FindFirstValue(ClaimTypes.Name);

            if (string.IsNullOrWhiteSpace(loggedInUsername)) return new Tuple<Item?, bool, bool, bool>(null, false, false, false);

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
                    Created = DateTime.UtcNow,
                };

                await context.Items.AddAsync(item);
                await context.SaveChangesAsync();
            }

            return new Tuple<Item?, bool, bool, bool>(item, userExists, categoryExists, true);
        }
    }
}
