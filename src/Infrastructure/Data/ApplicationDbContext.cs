using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using WeddingPlanner.Core.Domain.Entities;

namespace WeddingPlanner.Infrastructure.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<User>(options)
{
    public DbSet<Wedding> Weddings => Set<Wedding>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

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

            entity.HasOne(wedding => wedding.Coordinator)
                .WithMany(user => user.Weddings)
                .HasForeignKey(wedding => wedding.CoordinatorId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}