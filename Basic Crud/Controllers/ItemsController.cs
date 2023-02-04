using Basic_Crud.Models;
using Basic_Crud.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Basic_Crud.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ItemsController : ControllerBase
    {
        private readonly ItemsService service;

        public ItemsController(ItemsService service)
        {
            this.service = service;
        }

        [HttpGet]
        public async Task<ActionResult<List<Item>>> GetAll() => Ok(await service.GetAll());
        

        [HttpGet("{id}")]
        public async Task<ActionResult<Item>> GetAnItem([FromRoute] int id)
        {
            var item = await service.GetItem(id);

            if (item == null)
                return NotFound("Item with id " + id + " does not exist");

            return Ok(item);
        }

        [HttpPost]
        public async Task<ActionResult<Item>> CreateItem(ItemDto itemReq)
        {
            (Item? Item, bool loggedIn, bool userExists, bool categoryExists) = await service.CreateItem(itemReq);
            
            if (!loggedIn) return Unauthorized("Please make sure you are logged in before performing this action");
            if (!userExists) return NotFound("User does not exist!");
            if (!categoryExists) return NotFound("Category does not exist!");

            return Ok(Item);
        }
    }
}
