using BusinessObject.Entities;
using BusinessObject.Enums;
using Microsoft.EntityFrameworkCore;

namespace BusinessObject;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
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
    public DbSet<StationBatterySlot> StationBatterySlots { get; set; }
    public DbSet<BatteryBookingSlot> BatteryBookingSlots { get; set; }
    public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
    public DbSet<StaffAbsence> StaffAbsences { get; set; }
    public DbSet<StationStaffOverride> StationStaffOverrides { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

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

        modelBuilder.Entity<PasswordResetToken>()
            .HasIndex(p => new { p.Email, p.CreatedAt });

        modelBuilder.Entity<StaffAbsence>(e =>
        {
            e.ToTable("StaffAbsense");
            e.HasKey(x => x.StaffAbsenceId);
            e.HasIndex(x => new { x.UserId, x.Date }).IsUnique();
            e.HasIndex(x => x.Date);
            e.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETUTCDATE()");

        });

        modelBuilder.Entity<StationStaffOverride>(e =>
        {
            e.ToTable("StationStaffOverride");
            e.HasKey(x => x.StationStaffOverrideId);
            e.HasIndex(x => new { x.UserId, x.Date })
            .HasFilter("[shift_id] IS NULL")
            .IsUnique();

            e.HasIndex(x => new { x.UserId, x.Date, x.ShiftId })
             .HasFilter("[shift_id] IS NOT NULL")
             .IsUnique();

            e.HasIndex(x => new { x.StationId, x.Date });

            e.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETUTCDATE()");

        });
            

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
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Review>()
            .HasOne(r => r.BatterySwap)
            .WithMany(u => u.Reviews)
            .HasForeignKey(r => r.SwapId)
            .OnDelete(DeleteBehavior.NoAction);

        // Station -> Review relationship (One-to-Many) - NO CASCADE  
        modelBuilder.Entity<Review>()
            .HasOne(r => r.Station)
            .WithMany(u => u.Reviews)
            .HasForeignKey(r => r.StationId)
            .OnDelete(DeleteBehavior.Restrict);

        // BatterySwap relationships
        modelBuilder.Entity<BatterySwap>()
            .HasOne(bs => bs.User)
            .WithMany(u => u.BatterySwaps)
            .HasForeignKey(bs => bs.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<BatterySwap>()
            .HasOne(bs => bs.Station)
            .WithMany(s => s.BatterySwaps)
            .HasForeignKey(bs => bs.StationId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<BatterySwap>()
            .HasOne(bs => bs.StationStaff)
            .WithMany(s => s.BatterySwaps)
            .HasForeignKey(bs => bs.StationStaffId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<BatterySwap>()
            .HasOne(bs => bs.Vehicle)
            .WithMany(v => v.BatterySwaps)
            .HasForeignKey(bs => bs.VehicleId)
            .OnDelete(DeleteBehavior.NoAction);

        // SupportTicket relationships
        modelBuilder.Entity<SupportTicket>()
            .HasOne(st => st.User)
            .WithMany(u => u.SupportTickets)
            .HasForeignKey(st => st.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SupportTicket>()
            .HasOne(st => st.Station)
            .WithMany(s => s.SupportTickets)
            .HasForeignKey(st => st.StationId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Battery>()
            .HasOne(b => b.Vehicle)
            .WithMany(v => v.Batteries)
            .HasForeignKey(b => b.VehicleId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<User>()
            .Property(u => u.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        modelBuilder.Entity<Station>()
            .Property(s => s.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        modelBuilder.Entity<Booking>()
            .HasOne(b => b.Station)
            .WithMany(s => s.Bookings)
            .HasForeignKey(b => b.StationId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Booking>()
            .HasOne(b => b.Vehicle)
            .WithMany(v => v.Bookings)
            .HasForeignKey(b => b.VehicleId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Booking>()
            .HasOne(b => b.User)
            .WithMany(u => u.Bookings)
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Payment>()
            .HasOne(p => p.User)
            .WithMany(u => u.Payments)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Payment>()
            .HasOne(p => p.Booking)
            .WithMany(b => b.Payments)
            .HasForeignKey(p => p.BookingId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<StationBatterySlot>()
            .HasOne(sb => sb.Battery)
            .WithOne(b => b.StationBatterySlot)
            .HasForeignKey<StationBatterySlot>(sb => sb.BatteryId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<StationBatterySlot>()
            .HasOne(sb => sb.Station)
            .WithMany(s => s.StationBatterySlots)
            .HasForeignKey(sb => sb.StationId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<StationBatterySlot>()
            .Property(s => s.LastUpdated)
            .ValueGeneratedOnAddOrUpdate()
            .HasDefaultValueSql("GETUTCDATE()");

        modelBuilder.Entity<BatteryBookingSlot>()
            .HasOne(b => b.Battery)
            .WithMany(b => b.BatteryBookingSlots)
            .HasForeignKey(b => b.BatteryId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<BatteryBookingSlot>()
            .HasOne(b => b.Booking)
            .WithMany(b => b.BatteryBookingSlots)
            .HasForeignKey(b => b.BookingId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<BatteryBookingSlot>()
            .HasOne(b => b.StationBatterySlot)
            .WithMany(b => b.BatteryBookingSlots)
            .HasForeignKey(b => b.StationSlotId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<BatteryBookingSlot>()
            .Property(s => s.CreatedAt)
            .ValueGeneratedOnAddOrUpdate()
            .HasDefaultValueSql("GETUTCDATE()");

        modelBuilder.Entity<BatterySwap>()
            .HasOne(bs => bs.ToBattery)
            .WithMany(b => b.ToBatterySwaps)
            .HasForeignKey(bs => bs.ToBatteryId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<PasswordResetToken>()
            .HasOne(p => p.User)
            .WithMany(u => u.PasswordResetTokens)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<StaffAbsence>()
            .HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<Vehicle>()
            .HasMany(v => v.Batteries)
            .WithOne(b => b.Vehicle)
            .HasForeignKey(b => b.VehicleId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<StationStaffOverride>(e =>
        {
            e.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);


            e.HasOne(x => x.Station)
                .WithMany()
                .HasForeignKey(x => x.StationId)
                .OnDelete(DeleteBehavior.Cascade);
        });
            

        // ======= END OF FIX ALL FOREIGN KEY RELATIONSHIPS =======


        // ======= SEED DATA =======
        SeedData(modelBuilder);

        // ======= END OF SEED DATA =======
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
                Role = UserRole.Admin,
                Status = UserStatus.Active,
                Password = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                CreatedAt = DateTime.UtcNow
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
                SwapAmount = 10,
                Active = true,
                CreatedAt = DateTime.UtcNow
            },
            new SubscriptionPlan
            {
                PlanId = "plan-002",
                Name = "Premium Plan",
                Description = "Premium battery swap plan",
                MonthlyFee = 399000,
                SwapAmount = 20,
                Active = true,
                CreatedAt = DateTime.UtcNow
            }
        );
    }
}