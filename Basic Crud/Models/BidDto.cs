using System.ComponentModel.DataAnnotations.Schema;

namespace Basic_Crud.Models
{
    public class BidDto
    {
        public int Id { get; set; }
        public double Amount { get; set; }
        public string Bidder { get; set; }
        public Auction Auction { get; set; }
    }

    public class CreateBid
    {
        public double Amount { get; set; }
        public int AuctionId { get; set; }
    }
}
