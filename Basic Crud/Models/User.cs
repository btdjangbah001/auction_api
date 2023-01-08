using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Basic_Crud.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
        public ICollection<Item> Items { get; set; }
        public ICollection<Bid> Bids { get; set; }
    }
}
