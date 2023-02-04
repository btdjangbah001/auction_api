namespace Basic_Crud.Models
{
    public class AuctionDto
    {
        public int Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime? SoldDate { get; set; }
        public double Price { get; set; }
        public string? UserName { get; set; }
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public List<Bid> Bids { get; set; }
    }

    public class CreateAuction
    {
        public double Days { get; set; }
        public double Hours { get; set; }
        public double Minutes { get; set; }
        public double Price { get; set; }
        public int ItemId { get; set; }
    }
}
