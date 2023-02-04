using Basic_Crud.Data;
using Basic_Crud.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Basic_Crud.Services
{
    public class BidsService
    {
        private readonly AppDBContext context;
        private readonly UtilityService utilityService;

        public BidsService(AppDBContext context, UtilityService utilityService)
        {
            this.context = context;
            this.utilityService = utilityService;
        }

        public async Task<List<BidDto>> GetAllBids()
        {
            return await context.Bids.Select(b => new BidDto
            {
                Id = b.Id,
                Amount = b.Amount,
                Bidder = b.User.Username,
                Auction = b.Auction,
            }).ToListAsync();
        }
    }
}
