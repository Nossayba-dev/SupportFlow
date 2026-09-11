using Microsoft.EntityFrameworkCore;
using SupportFlow.Models;
namespace SupportFlow.Data
{
    public class SupportFlowDbContext : DbContext
    {
        public SupportFlowDbContext(DbContextOptions<SupportFlowDbContext> options) : base(options)
        {

        }
        //the tables of our database
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
    }
}
