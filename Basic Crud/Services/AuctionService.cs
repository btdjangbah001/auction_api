using Basic_Crud.Data;
using Basic_Crud.Models;
using Basic_Crud.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Basic_Crud.Services
{
    public class AuctionService
    {
        private readonly AppDBContext context;
        private readonly AuctionRepository auctionRepo;
        private readonly UtilityService utilityService;

        public AuctionService(AppDBContext context,AuctionRepository auctionRepo , UtilityService utilityService)
        {
            this.context = context;
            this.auctionRepo = auctionRepo;
            this.utilityService = utilityService;
        }

        public async Task<List<AuctionDto>> GetAll()
        {
            return await auctionRepo.FindAll();
        }

        public async Task<AuctionDto?> GetAuction(int id)
        {
            return await auctionRepo.FindOneById(id);
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

            Auction? openedAuction = await auctionRepo.FindOpenedAuctionForItem(item.Id);
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

            auctionRepo.SaveOne(auction);

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

            auction = await auctionRepo.FindAuctionById(id);

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

            auctionRepo.DeleteAuction(auction);

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

        public async Task<List<Bid>> GetAuctionBids(int id)
        {
            return await auctionRepo.GetAuctionBids(id);
        }
    }
}
