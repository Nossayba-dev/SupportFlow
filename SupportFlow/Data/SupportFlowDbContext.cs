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
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Ticket -> Category : required, delete refused if tickets exist
            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Category)
                .WithMany(c => c.Tickets)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            // Ticket -> User : à toi de l'écrire
            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.User)
                .WithMany(u => u.Tickets)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
