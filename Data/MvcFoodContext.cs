using Microsoft.EntityFrameworkCore;
using Restaurant_Website.Models;

namespace Restaurant_Website.Data
{
    public class MvcFoodContext : DbContext
    {
        public MvcFoodContext(DbContextOptions<MvcFoodContext> options) : base(options) { }

        public DbSet<Food> Food { get; set; }
    }
}