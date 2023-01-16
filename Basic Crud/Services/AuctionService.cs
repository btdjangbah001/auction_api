using Basic_Crud.Data;
using Basic_Crud.Models;
using Microsoft.EntityFrameworkCore;

namespace Basic_Crud.Services
{
    public class AuctionService
    {
        private readonly AppDBContext context;

        public AuctionService(AppDBContext context)
        {
            this.context = context;
        }

        public async Task<List<Auction>> GetAll()
        {
            return await context.Auctions.ToListAsync();
        }

        public async Task<Auction?> GetAuction(int id)
        {
            return await context.Auctions.FindAsync(id);
        }
    }
}
