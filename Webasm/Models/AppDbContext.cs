using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UserApp.Models;
using UsersApp.Models;

namespace UsersApp.Models
{
    public class AppDbContext : IdentityDbContext<Users>
    {
        public AppDbContext(DbContextOptions options) : base(options)
        { }


            public DbSet<UserProfile> UserProfiles { get; set; }

       



        public DbSet<Appointments> AspNetAppointments { get; set; }
        public DbSet<Product> Products { get; set; }

        public string Title { get; set; }
        public DateTime Start { get; set; }

       

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // ⚡ Keep Identity configurations

            // Explicit precision for Product.Price
            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasColumnType("decimal(18,2)");
        }
    }
}
