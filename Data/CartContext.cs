using Microsoft.EntityFrameworkCore;
using Restaurant_Website.Models;

namespace Restaurant_Website.Data
{
    public class CartContext : DbContext
    {
        public CartContext(DbContextOptions<CartContext> options) : base(options) { }

        public DbSet<Cart> Carts { get; set; }
    }
}