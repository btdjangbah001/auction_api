namespace Basic_Crud.Models
{
    public class Item
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public User Owner { get; set; }
        public string Description { get; set; }
        public Boolean Sold { get; set; }
        public DateTime Created { get; set; }
        public Category Category { get; set; }
        public ICollection<Auction> Auctions { get; set; }
    }
}
