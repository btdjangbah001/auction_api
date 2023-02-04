using Basic_Crud.Data;
using Basic_Crud.Models;
using Microsoft.EntityFrameworkCore;

namespace Basic_Crud.Services
{
    public class AuctionService
    {
        private readonly AppDBContext context;
        private readonly UtilityService utilityService;

        public AuctionService(AppDBContext context, UtilityService utilityService)
        {
            this.context = context;
            this.utilityService = utilityService;
        }

        public async Task<List<AuctionDto>> GetAll()
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

        public async Task<AuctionDto?> GetAuction(int id)
        {
            return await context.Auctions.Where(a => a.Id == id).Select(a => new AuctionDto
            {
                Id= a.Id,
                StartDate = a.StartDate,
                EndDate = a.EndDate,
                SoldDate = a.SoldDate,
                Price = a.Price,
                UserName = a.Winner != null ? a.Winner.Username : null,
                ItemName = a.Item.Name,
                Bids = a.Bids
            }).FirstOrDefaultAsync();
        }

        public async Task<(Auction?, bool, bool, bool, bool, bool)> CreateAuction(CreateAuction auctionDto)
        {
            string? loggedInUser = utilityService.GetLoggedInUser();
            User? user = null;
            Item? item = null;
            bool loggedIn, userExist, itemExists, userOwnsItem, itemNotSold;
            loggedIn = userExist = itemExists = userOwnsItem = itemNotSold = false;

            if (loggedInUser == null) return (null, loggedIn, userExist, itemExists, userOwnsItem, itemNotSold);
            loggedIn = true;

            user = await context.Users.Where(q => q.Username == loggedInUser).FirstOrDefaultAsync();
            item = await context.Items.FindAsync(auctionDto.ItemId);

            if (user == null) return (null, loggedIn, userExist, itemExists, userOwnsItem, itemNotSold);
            userExist = true;
            if (item == null) return (null, loggedIn, userExist, itemExists, userOwnsItem, itemNotSold);
            itemExists = true;
            if (user.Id != item.Owner.Id) return (null, loggedIn, userExist, itemExists, userOwnsItem, itemNotSold);
            userOwnsItem = true;
            if (item.Sold) return (null, loggedIn, userExist, itemExists, userOwnsItem, itemNotSold);
            itemNotSold = true;

            var auction = new Auction
            {
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(auctionDto.Days).AddHours(auctionDto.Hours).AddMinutes(auctionDto.Minutes),
                Price = auctionDto.Price,
                Winner = null,
                Item = item,
                Bids = new List<Bid>(),
            };

            await context.Auctions.AddAsync(auction);
            await context.SaveChangesAsync();

            return (auction, loggedIn, userExist, itemExists, userOwnsItem, itemNotSold);
        }
    }
}
