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
        public async Task<ActionResult<List<Auction>>> GetAll() => Ok(await service.GetAll());

        [HttpGet("{id}")]
        public async Task<ActionResult<Auction>> GetAuction([FromRoute] int id)
        {
            AuctionDto? auction = await service.GetAuction(id); 

            if (auction == null)
                return NotFound("Auction with id " + id + " does not exist!");

            return Ok(auction);
        }

        [HttpPost]
        public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuction auctionDto)
        {
            (Auction? auction, bool loggedIn, bool userExist, bool itemExists, bool userOwnsItem, bool itemSold, bool hasOpenedAuction) 
                = await service.CreateAuction(auctionDto);
            if (!loggedIn) return Unauthorized("Please log in again to perform this action");
            if (!userExist) return NotFound("This user does not exists!");
            if (!itemExists) return NotFound("This item with id " + auctionDto.ItemId + " does not exists!");
            if (!userOwnsItem) return BadRequest("Item with id " + auctionDto.ItemId + " does not belong to you!");
            if (itemSold) return BadRequest("You cannot auction off an item that has already been sold.");
            if (hasOpenedAuction) return BadRequest("This item already has an opened auction.");
            return Ok(auction);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<AuctionDto>> DeleteAuction([FromRoute] int id)
        {
            (AuctionDto? auction, bool loggedIn, bool userExist, bool auctionExists, bool userOwnsAuction, bool itemSold) 
                = await service.DeleteAuction(id);

            if (!loggedIn) return Unauthorized("Please log in again to perform this action");
            if (!userExist) return NotFound("This user does not exists!");
            if (!auctionExists) return NotFound("This auction with id " + id + " does not exists!");
            if (!userOwnsAuction) return BadRequest("Auction with id " + id + " does not belong to you!");
            if (itemSold) return BadRequest("This auction has already been sold and closed");
            return Ok(auction);
        }

        [HttpGet("{id}/Bids")]
        public async Task<ActionResult<List<Bid>>> GetAuctionBid([FromRoute] int id)
        {
            return Ok(await service.GetAuctionBids(id));
        }
    }
}
