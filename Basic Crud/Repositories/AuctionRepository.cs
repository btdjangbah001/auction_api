using Basic_Crud.Data;
using Basic_Crud.Models;
using Microsoft.EntityFrameworkCore;

namespace Basic_Crud.Repositories
{
    public class AuctionRepository
    {
        private readonly AppDBContext context;

        public AuctionRepository(AppDBContext context)
        {
            this.context = context;
        }

        public async Task<List<AuctionDto>> FindAll()
        {
            return await context.Auctions.Select(a => new AuctionDto
            {
                Id = a.Id,
                StartDate = a.StartDate,
                EndDate = a.EndDate,
                SoldDate = a.SoldDate,
                Price = a.Price,
                UserName = a.Winner != null ? a.Winner.Username : null,
                ItemName = a.Item.Name,
                Bids = a.Bids
            }).ToListAsync();
        }

        public async Task<AuctionDto?> FindOneById(int id)
        {
            return await context.Auctions.Where(a => a.Id == id).Select(a => new AuctionDto
            {
                Id = a.Id,
                StartDate = a.StartDate,
                EndDate = a.EndDate,
                SoldDate = a.SoldDate,
                Price = a.Price,
                UserName = a.Winner != null ? a.Winner.Username : null,
                ItemName = a.Item.Name,
                Bids = a.Bids
            }).FirstOrDefaultAsync();
        }

        public async Task<Auction?> FindAuctionById(int id)
        {
            return await context.Auctions.Where(a => a.Id == id).Select(a => new Auction
            {
                Id = a.Id,
                StartDate = a.StartDate,
                EndDate = a.EndDate,
                SoldDate = a.SoldDate,
                Winner = a.Winner,
                Item = a.Item,
                Bids = a.Bids,
            }).FirstAsync();
        }

        public async void SaveOne(Auction auction)
        {
            await context.Auctions.AddAsync(auction);
            await context.SaveChangesAsync();
        }

        public async void DeleteAuction(Auction auction)
        {
            context.Auctions.Attach(auction);
            context.Auctions.Remove(auction);
            await context.SaveChangesAsync();
        }

        public async Task<Auction?> FindOpenedAuctionForItem(int itemId)
        {
            return await context.Auctions.Where(a => a.Item.Id == itemId && a.EndDate > DateTime.UtcNow).FirstOrDefaultAsync();
        }

        public async Task<List<Bid>> GetAuctionBids(int id)
        {
            return await context.Bids.Where(b => b.Auction.Id == id).ToListAsync();
        }
    }
}
