using System.ComponentModel.DataAnnotations.Schema;

namespace Basic_Crud.Models
{
    public class Bid
    {
        public int Id { get; set; }
        public double Amount { get; set; }

        [Column(name:"Bidder")]
        public User User { get; set; }
        public Auction Auction { get; set; }
    }
}
