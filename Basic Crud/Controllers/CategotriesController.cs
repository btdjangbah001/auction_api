using Basic_Crud.Models;
using Basic_Crud.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Basic_Crud.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CategotriesController : ControllerBase
    {
        private readonly CategoriesService service;

        public CategotriesController(CategoriesService service)
        {
            this.service = service;
        }

        [HttpPost]
        public async Task<ActionResult<Category>> CreateCategory(CategoryDto categoryDto)
        {
            return await service.CreateCategory(categoryDto);
        }
    }
}
