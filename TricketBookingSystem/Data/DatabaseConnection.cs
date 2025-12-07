using Microsoft.EntityFrameworkCore;
using TricketBookingSystem.Models;

namespace TricketBookingSystem.Data
{
    public class DatabaseConnection: DbContext
    {
        public DatabaseConnection(DbContextOptions<DatabaseConnection> options) : base(options)
        {

        }
        public DbSet<Users> Users { get; set; }
        public DbSet<Tricket> Tricket { get; set; }
        public DbSet<UserTokenSelection> UserTokenSelection { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            //Database Entity
            modelBuilder.Entity<Users>().HasKey(x => x.UserId);
            modelBuilder.Entity<Tricket>().HasKey(x => x.Id);
            modelBuilder.Entity<UserTokenSelection>().HasKey(x => x.Id);
            

            base.OnModelCreating(modelBuilder);
        }
    }
}
