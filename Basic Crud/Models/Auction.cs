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
<<<<<<< Updated upstream
        public int UserId { get; set; }

        [Column(name:"Winner")]
        public User? User { get; set; }
        public int ItemId { get; set; }
=======
        public User? Winner { get; set; }
>>>>>>> Stashed changes
        public Item Item { get; set; }
        public ICollection<Bid> Bids { get; set; }
    }
}
