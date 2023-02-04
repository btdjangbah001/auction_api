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

        public async Task<(Auction?, bool, bool, bool, bool, bool, bool)> CreateAuction(CreateAuction auctionDto)
        {
            Item? item = null;
            bool itemExists, userOwnsItem, itemSold, itemHasOpenedAuction;
            itemExists = userOwnsItem = itemSold = itemHasOpenedAuction = false;

            (User? user, bool loggedIn, bool userExist) = await utilityService.GetLoggedInUserDetails();

            if (loggedIn == false) 
                return (null, loggedIn, userExist, itemExists, userOwnsItem, itemSold, itemHasOpenedAuction);

            if (userExist == false) 
                return (null, loggedIn, userExist, itemExists, userOwnsItem, itemSold, itemHasOpenedAuction);

            item = await context.Items.FindAsync(auctionDto.ItemId);

            if (item == null) 
                return (null, loggedIn, userExist, itemExists, userOwnsItem, itemSold, itemHasOpenedAuction);
            itemExists = true;

            if (user!.Id != item.Owner.Id) 
                return (null, loggedIn, userExist, itemExists, userOwnsItem, itemSold, itemHasOpenedAuction);
            userOwnsItem = true;

            if (item.Sold)
            {
                itemSold = true;
                return (null, loggedIn, userExist, itemExists, userOwnsItem, itemSold, itemHasOpenedAuction);

            } 

            Auction? openedAuction = await context.Auctions.Where(a => a.Item.Id == item.Id && a.EndDate > DateTime.UtcNow).FirstOrDefaultAsync();
            if (openedAuction != null)
            {
                itemHasOpenedAuction = true;
                return (null, loggedIn, userExist, itemExists, userOwnsItem, itemSold, itemHasOpenedAuction);
            }   


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

            return (auction, loggedIn, userExist, itemExists, userOwnsItem, itemSold, itemHasOpenedAuction);
        }

        public async Task<(AuctionDto?, bool, bool, bool, bool, bool)> DeleteAuction(int id)
        {
            Auction? auction = null;
            bool auctionExists, userOwnsItem, itemSold;
            auctionExists = userOwnsItem = itemSold = false;

            (User? user, bool loggedIn, bool userExist) = await utilityService.GetLoggedInUserDetails();

            if (loggedIn == false)
                return (null, loggedIn, userExist, auctionExists, userOwnsItem, itemSold);

            if (userExist == false)
                return (null, loggedIn, userExist, auctionExists, userOwnsItem, itemSold);

            auction = await context.Auctions.Where(a => a.Id == id).Select(a => new Auction
            {
                Id = a.Id,
                StartDate = a.StartDate,
                EndDate = a.EndDate,
                SoldDate = a.SoldDate,
                Winner = a.Winner,
                Item = a.Item,
                Bids = a.Bids,
            }).FirstAsync();

            if (auction == null)
                return (null, loggedIn, userExist, auctionExists, userOwnsItem, itemSold);
            auctionExists = true;

            if (auction.Item.OwnerId != user!.Id)
                return (null, loggedIn, userExist, auctionExists, userOwnsItem, itemSold);
            userOwnsItem = true;

            if (auction.Item.Sold)
            {
                itemSold = true;
                return (null, loggedIn, userExist, auctionExists, userOwnsItem, itemSold);

            }

            context.Auctions.Attach(auction);
            context.Auctions.Remove(auction);
            await context.SaveChangesAsync();

            return (new AuctionDto
            {
                Id = auction.Id,
                StartDate = auction.StartDate,
                EndDate = auction.EndDate,
                Price = auction.Price,
                ItemId = auction.Item.Id,
                ItemName = auction.Item.Name,
                Bids = auction.Bids,
            }, loggedIn, userExist, auctionExists, userOwnsItem, itemSold);
        }
    }
}
