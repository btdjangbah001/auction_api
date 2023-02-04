using System.ComponentModel.DataAnnotations.Schema;

namespace Basic_Crud.Models
{
    public class Auction
    {
        public int Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime? SoldDate { get; set; }
        public double Price { get; set; }
        public User? Winner { get; set; }
        public Item Item { get; set; }
        public List<Bid> Bids { get; set; }
    }
}
