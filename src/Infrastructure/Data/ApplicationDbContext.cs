using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WeddingPlanner.Core.Domain.Entities;

namespace WeddingPlanner.Infrastructure.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<User>(options)
{
    public DbSet<Wedding> Weddings => Set<Wedding>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Wedding>(entity =>
        {
            entity.Property(wedding => wedding.Name).HasMaxLength(150).IsRequired();
            entity.Property(wedding => wedding.Venue).HasMaxLength(200);
            entity.Property(wedding => wedding.Status).HasConversion<string>().HasMaxLength(30);

            entity.HasOne(wedding => wedding.Owner)
                .WithMany(user => user.Weddings)
                .HasForeignKey(wedding => wedding.OwnerId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}