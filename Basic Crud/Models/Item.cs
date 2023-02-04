namespace Basic_Crud.Models
{
    public class Item
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int OwnerId { get; set; }
        public User Owner { get; set; }
        public string Description { get; set; }
        public Boolean Sold { get; set; }
        public DateTime Created { get; set; }
        public Category Category { get; set; }
        public List<Auction> Auctions { get; set; }
    }
}
