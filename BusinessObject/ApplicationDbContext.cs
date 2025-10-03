using BusinessObject.Entities;
using Microsoft.EntityFrameworkCore;
using BusinessObject.Enums;

namespace BusinessObject
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // DbSets
        public DbSet<User> Users { get; set; }
        public DbSet<Station> Stations { get; set; }
        public DbSet<BatteryType> BatteryTypes { get; set; }
        public DbSet<Battery> Batteries { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<StationStaff> StationStaffs { get; set; }
        public DbSet<BatterySwap> BatterySwaps { get; set; }
        public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<SubscriptionPayment> SubscriptionPayments { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<SupportTicket> SupportTickets { get; set; }
        public DbSet<StationInventory> StationInventories { get; set; }
        public DbSet<Reservation> Reservations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure composite key for Booking
            modelBuilder.Entity<Booking>()
                .HasKey(b => new { b.StationId, b.UserId, b.VehicleId, b.BatteryId, b.BatteryTypeId });

            // Configure unique indexes
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Battery>()
                .HasIndex(b => b.SerialNo)
                .IsUnique();

            modelBuilder.Entity<Vehicle>()
                .HasIndex(v => v.LicensePlate)
                .IsUnique();

            // ======= FIX ALL FOREIGN KEY RELATIONSHIPS =======

            // User -> Station relationship (One-to-Many)
            modelBuilder.Entity<Station>()
                .HasOne(s => s.User)
                .WithMany(u => u.Stations)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // User -> Review relationship (One-to-Many) - NO CASCADE
            modelBuilder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Station -> Review relationship (One-to-Many) - NO CASCADE  
            modelBuilder.Entity<Review>()
                .HasOne(r => r.Station)
                .WithMany(s => s.Reviews)
                .HasForeignKey(r => r.StationId)
                .OnDelete(DeleteBehavior.Restrict);

            // Fix other problematic relationships based on shadow properties

            // BatterySwap relationships
            modelBuilder.Entity<BatterySwap>()
                .HasOne<User>()
                .WithMany(u => u.BatterySwaps)
                .HasForeignKey("UserId")
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BatterySwap>()
                .HasOne<Station>()
                .WithMany(s => s.BatterySwaps)
                .HasForeignKey("StationId")
                .OnDelete(DeleteBehavior.Restrict);

            // SubscriptionPayment relationships
            modelBuilder.Entity<SubscriptionPayment>()
                .HasOne<User>()
                .WithMany(u => u.SubscriptionPayments)
                .HasForeignKey("UserId")
                .OnDelete(DeleteBehavior.Restrict);

            // SupportTicket relationships
            modelBuilder.Entity<SupportTicket>()
                .HasOne<User>()
                .WithMany(u => u.SupportTickets)
                .HasForeignKey("UserId")
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SupportTicket>()
                .HasOne<Station>()
                .WithMany(s => s.SupportTickets)
                .HasForeignKey("StationId")
                .OnDelete(DeleteBehavior.Restrict);

            // Vehicle -> Battery relationship (nullable)
            modelBuilder.Entity<Vehicle>()
                .HasOne<Battery>()
                .WithMany()
                .HasForeignKey("BatteryId")
                .OnDelete(DeleteBehavior.SetNull);

            // Configure default values
            modelBuilder.Entity<User>()
                .Property(u => u.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder.Entity<Station>()
                .Property(s => s.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // Seed data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed admin user
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    UserId = "admin-001",
                    FullName = "System Administrator",
                    Email = "admin@evdriver.com",
                    Phone = "0123456789",
                    Role = UserRole.Admin, // Admin
                    Status = UserStatus.Active, // Active
                    Password = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );

            // Seed battery types
            modelBuilder.Entity<BatteryType>().HasData(
                new BatteryType { BatteryTypeId = "type-001", BatteryTypeName = "Standard Li-ion" },
                new BatteryType { BatteryTypeId = "type-002", BatteryTypeName = "High Capacity Li-ion" },
                new BatteryType { BatteryTypeId = "type-003", BatteryTypeName = "Fast Charge Li-ion" }
            );

            // Seed subscription plans
            modelBuilder.Entity<SubscriptionPlan>().HasData(
                new SubscriptionPlan
                {
                    PlanId = "plan-001",
                    Name = "Basic Plan",
                    Description = "Basic battery swap plan",
                    MonthlyFee = 199000,
                    SwapsIncluded = "10",
                    Active = true,
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new SubscriptionPlan
                {
                    PlanId = "plan-002",
                    Name = "Premium Plan",
                    Description = "Premium battery swap plan",
                    MonthlyFee = 399000,
                    SwapsIncluded = "25",
                    Active = true,
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );
        }
    }
}