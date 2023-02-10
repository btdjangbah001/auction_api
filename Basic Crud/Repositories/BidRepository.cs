using Basic_Crud.Data;
using Basic_Crud.Models;
using Microsoft.EntityFrameworkCore;

namespace Basic_Crud.Repositories
{
    public class BidRepository
    {
        private readonly AppDBContext context;

        public BidRepository(AppDBContext context)
        {
            this.context = context;
        }

        public async Task<List<BidDto>> FindAllBids()
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
