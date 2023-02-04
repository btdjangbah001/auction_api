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
    public class AuctionController : ControllerBase
    {
        private readonly AuctionService service;

        public AuctionController(AuctionService service)
        {
            this.service = service;
        }

        [HttpGet]
        public async Task<ActionResult<List<Auction>>> GetAll()
        {
            return Ok(await service.GetAll());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Auction>> GetAuction([FromRoute] int id)
        {
            var auction = await service.GetAuction(id); 

            if (auction == null)
            {
                return NotFound("Auction with id " + id + " does not exist!");
            }

            return Ok(auction);
        }

        [HttpPost]
        public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuction auctionDto)
        {
            (Auction? auction, bool loggedIn, bool userExist, bool itemExists, bool userOwnsItem, bool itemNotSold) 
                = await service.CreateAuction(auctionDto);
            if (!loggedIn) return Unauthorized("Please log in again to perform this action");
            if (!userExist) return NotFound("This user does not exists!");
            if (!itemExists) return NotFound("This item with id " + auctionDto.ItemId + " does not exists!");
            if (!userOwnsItem) return Unauthorized("Item with id " + auctionDto.ItemId + " does not belong to you!");
            if (!itemNotSold) return Unauthorized("You cannot auction off an item that has already been sold.");
            return Ok(auction);
        }
    }
}
