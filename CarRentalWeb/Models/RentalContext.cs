using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace VehicleRentalWeb.Models
{
    public class RentalContext : IdentityDbContext<ApplicationUser>
    {
        public RentalContext(DbContextOptions options)
            : base(options)
        {
        }

        // Tables
        public DbSet<Vehicle> Vehicles { get; set; } = null!;
        public DbSet<Car> Cars { get; set; } = null!;
        public DbSet<Bike> Bikes { get; set; } = null!;
        public DbSet<Truck> Trucks { get; set; } = null!;
        public DbSet<Customer> Customers { get; set; } = null!;
        public DbSet<Rental> Rentals { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Vehicle inheritance
            modelBuilder.Entity<Vehicle>()
                .HasDiscriminator<string>("VehicleType")
                .HasValue<Car>("Car")
                .HasValue<Bike>("Bike")
                .HasValue<Truck>("Truck");

            // Rate precision
            modelBuilder.Entity<Vehicle>()
                .Property(v => v.RatePerDay)
                .HasPrecision(18, 2);

            // Customer <=> User
            modelBuilder.Entity<Customer>()
                .HasOne(c => c.User)
                .WithOne()
                .HasForeignKey<Customer>(c => c.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Rental relationships
            modelBuilder.Entity<Rental>()
                .HasOne(r => r.Customer)
                .WithMany()
                .HasForeignKey(r => r.CustomerId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Rental>()
                .HasOne(r => r.Vehicle)
                .WithMany()
                .HasForeignKey(r => r.VehicleId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
