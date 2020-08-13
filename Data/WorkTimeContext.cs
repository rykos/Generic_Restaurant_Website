using Microsoft.EntityFrameworkCore;
using Restaurant_Website.Models;

namespace Restaurant_Website.Data
{
    public class WorkTimeContext : DbContext
    {
        public WorkTimeContext(DbContextOptions<WorkTimeContext> options) : base(options) { }

        public DbSet<WorkTime> WorkTimes { get; set; }
    }
}