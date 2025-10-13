using Microsoft.EntityFrameworkCore;
using TrainGenie.Models;

namespace RailVision.App.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<WagonDetachmentLog> WagonDetachmentLogs { get; set; }
        public DbSet<User> Users { get; set; }
    }
}