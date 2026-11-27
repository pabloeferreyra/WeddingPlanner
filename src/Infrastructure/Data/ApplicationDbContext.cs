using System.Text.Json;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using WeddingPlanner.Core.Domain.Entities;
using WeddingPlanner.Core.Domain.Enums;

namespace WeddingPlanner.Infrastructure.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<User>(options)
{
    public DbSet<Budget> Budgets => Set<Budget>();
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<ChecklistItem> ChecklistItems => Set<ChecklistItem>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<Vendor> Vendors => Set<Vendor>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Guest> Guests => Set<Guest>();
    public DbSet<Wedding> Weddings => Set<Wedding>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<Timeline> TimeLines => Set<Timeline>();
    public DbSet<TaskItem> TaskItems => Set<TaskItem>();
    public DbSet<VendorPayment> VendorPayments => Set<VendorPayment>();
    public DbSet<WeddingTable> WeddingTables => Set<WeddingTable>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateTime>()
            .HaveColumnType("timestamp without time zone");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Guest>()
            .Property(guest => guest.CompanionStatus)
            .HasDefaultValue(CompanionAttendanceStatus.Pending);

        modelBuilder.Entity<WeddingTable>(entity =>
        {
            entity.HasAlternateKey(table => new { table.Id, table.WeddingId });
            entity.HasIndex(table => new { table.WeddingId, table.Name }).IsUnique();
            entity.ToTable(table => table.HasCheckConstraint("CK_WeddingTables_Capacity", "\"Capacity\" > 0"));
            entity.HasOne(table => table.Wedding)
                .WithMany()
                .HasForeignKey(table => table.WeddingId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Guest>()
            .HasOne(guest => guest.WeddingTable)
            .WithMany(table => table.Guests)
            .HasForeignKey(guest => new { guest.WeddingTableId, guest.WeddingId })
            .HasPrincipalKey(table => new { table.Id, table.WeddingId })
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<VendorPayment>()
            .HasOne(payment => payment.VendorAssignment)
            .WithMany(assignment => assignment.Payments)
            .HasForeignKey(payment => payment.VendorAssignmentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<VendorDocument>(entity =>
        {
            entity.HasOne(document => document.Vendor)
                .WithMany(vendor => vendor.Documents)
                .HasForeignKey(document => document.VendorId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.ToTable(table => table.HasCheckConstraint("CK_VendorDocument_Storage",
                "(\"FileUrl\" IS NOT NULL AND \"StorageKey\" IS NULL) OR " +
                "(\"FileUrl\" IS NULL AND \"StorageKey\" IS NOT NULL AND \"VendorAssignmentId\" IS NOT NULL AND \"FileName\" IS NOT NULL AND \"ContentType\" IS NOT NULL AND \"FileSize\" IS NOT NULL AND \"FileSize\" > 0)"));
        });

        modelBuilder.Entity<Budget>()
            .Property(budget => budget.CategoryAllocations)
            .HasConversion(
                value => JsonSerializer.Serialize(value, (JsonSerializerOptions?)null),
                json => JsonSerializer.Deserialize<Dictionary<string, decimal>>(json, (JsonSerializerOptions?)null) ?? new(),
                new ValueComparer<Dictionary<string, decimal>>(
                    (a, b) => a != null && b != null && a.Count == b.Count && !a.Except(b).Any(),
                    d => d.Aggregate(0, (hash, kv) => HashCode.Combine(hash, kv.Key, kv.Value)),
                    d => new Dictionary<string, decimal>(d)));

        modelBuilder.Entity<Wedding>(entity =>
        {
            entity.Property(wedding => wedding.Title).HasMaxLength(150).IsRequired();
            entity.Property(wedding => wedding.Status).HasConversion<string>().HasMaxLength(30);

            entity.OwnsOne(wedding => wedding.Bride);
            entity.OwnsOne(wedding => wedding.Groom);
            entity.OwnsOne(wedding => wedding.Celebrated);

            entity.HasOne(wedding => wedding.Coordinator)
                .WithMany(user => user.Weddings)
                .HasForeignKey(wedding => wedding.CoordinatorId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}