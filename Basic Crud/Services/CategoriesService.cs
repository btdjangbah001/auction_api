using Basic_Crud.Data;
using Basic_Crud.Models;

namespace Basic_Crud.Services
{
    public class CategoriesService
    {
        private readonly AppDBContext context;
        private readonly IConfiguration configuration;

        public CategoriesService(AppDBContext context, IConfiguration configuration)
        {
            this.context = context;
            this.configuration = configuration;
        }

        public async Task<Category> CreateCategory(CategoryDto categoryDto)
        {
            var category = new Category
            {
                Name = categoryDto.Name,
                Description = categoryDto.Description,
            };

            await context.Categories.AddAsync(category);
            await context.SaveChangesAsync();
            return category;
        }
    }
}
