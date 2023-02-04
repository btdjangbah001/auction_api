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
    public class BidsController : ControllerBase
    {
        private readonly BidsService service;

        public BidsController(BidsService service)
        {
            this.service = service;
        }

        [HttpGet]
        public async Task<List<BidDto>> GetAllBids()
        {
            return await service.GetAllBids();
        }

        [HttpPost]
        public async Task<ActionResult<Bid>> MakeBid(CreateBid bidDto)
        {
            (Bid? bid, bool loggedIn, bool userExists, bool auctionExists, bool auctionNotExpired) = await service.MakeBid(bidDto);
            if (!loggedIn) return Unauthorized("Pease log in again to perform this action");
            if (!userExists) return NotFound("This user does not exists!");
            if (!auctionExists) return NotFound("Auction with id " + bidDto.AuctionId + " does not exist!");
            if (!auctionNotExpired) return BadRequest("Auction with id " + bidDto.AuctionId + " has expired");
            return Ok(bid);
        }
    }
}
