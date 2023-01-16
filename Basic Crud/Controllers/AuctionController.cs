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
    }
}
