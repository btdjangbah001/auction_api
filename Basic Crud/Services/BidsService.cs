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

        public  async Task<(Bid?, bool, bool, bool, bool)> MakeBid(CreateBid bidDto)
        {
            string? loggedInUser = utilityService.GetLoggedInUser();
            User? user = null;
            Auction? auction = null;

            bool loggedIn, userExists, auctionExists, auctionNotExpired; 
            loggedIn = userExists = auctionExists = auctionNotExpired = false;

            if (loggedInUser == null) return (null, loggedIn, userExists, auctionExists, auctionNotExpired);
            loggedIn = true;

            user = await context.Users.Where(u => u.Username == loggedInUser).FirstOrDefaultAsync();

            if (user == null) return (null, loggedIn, userExists, auctionExists, auctionNotExpired);
            userExists = true;

            auction = await context.Auctions.FindAsync(bidDto.AuctionId);

            if (auction == null) return (null, loggedIn, userExists, auctionExists, auctionNotExpired);
            auctionExists = true;
    
            if (auction.EndDate < DateTime.UtcNow) return (null, loggedIn, userExists, auctionExists, auctionNotExpired);
            auctionNotExpired = true;

            Bid bid = new Bid
            {
                Amount = bidDto.Amount,
                User = user,
                Auction = auction,
            };

            await context.Bids.AddAsync(bid);
            await context.SaveChangesAsync();

            return (bid, loggedIn, userExists, auctionExists, auctionNotExpired);
        }
    }
}
