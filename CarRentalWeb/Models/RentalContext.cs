using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

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

        // Optional: Inheritance mapping for Vehicle hierarchy
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Vehicle>()
                .HasDiscriminator<string>("VehicleType")
                .HasValue<Car>("Car")
                .HasValue<Bike>("Bike")
                .HasValue<Truck>("Truck");
        }
    }
}
