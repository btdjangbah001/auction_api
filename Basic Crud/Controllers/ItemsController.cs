﻿using Basic_Crud.Models;
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
        public async Task<ActionResult<IEnumerable<Item>>> GetAll()
        {
            return Ok(await service.GetAll());
        }

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
            string? username = HttpContext.User.FindFirst(ClaimTypes.Name).Value;

            if (username == null) 
                return BadRequest("Please make sure you are logged in before performing this action");

            var res = await service.CreateItem(itemReq, username);

            if (res == null) return BadRequest("Something went wrong, please try again.");
            if (res.Item2 == false) return BadRequest("Please make sure you are logged in before performing this action");
            if (res.Item3 == false) return BadRequest("Please make sure you have a valid category");

            return Ok(res.Item1);
        }
    }
}
