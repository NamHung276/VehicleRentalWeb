using Microsoft.EntityFrameworkCore;

namespace VehicleRentalWeb.Models
{
    public class RentalContext : DbContext
    {
        public RentalContext(DbContextOptions<RentalContext> options)
            : base(options)
        {
        }

        // DbSets (tables)
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Car> Cars { get; set; }
        public DbSet<Bike> Bikes { get; set; }
        public DbSet<Truck> Trucks { get; set; }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Rental> Rentals { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Vehicle inheritance
            modelBuilder.Entity<Vehicle>()
                .HasDiscriminator<string>("VehicleType")
                .HasValue<Car>("Car")
                .HasValue<Bike>("Bike")
                .HasValue<Truck>("Truck");

            // User–Customer (1-to-1)
            modelBuilder.Entity<Customer>()
                .HasOne(c => c.User)
                .WithOne(u => u.Customer)
                .HasForeignKey<Customer>(c => c.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Customer–Rental (1-to-many)
            modelBuilder.Entity<Rental>()
                .HasOne(r => r.Customer)
                .WithMany()
                .HasForeignKey(r => r.CustomerId)
                .OnDelete(DeleteBehavior.SetNull);
        }

    }
}
