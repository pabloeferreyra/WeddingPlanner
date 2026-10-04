using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WeddingPlanner.Core.Domain.Entities;

namespace WeddingPlanner.Core.EntityTypeConfigurations;

public class WeddingConfiguration : IEntityTypeConfiguration<Wedding>
{
    public void Configure(EntityTypeBuilder<Wedding> builder)
    {
        builder.HasKey(w => w.Id);

        builder.Property(w => w.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(w => w.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.OwnsOne(w => w.Location);

        builder.HasOne(w => w.Coordinator)
            .WithMany(u => u.Weddings)
            .HasForeignKey(w => w.CoordinatorId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(w => w.Guests)
            .WithOne(g => g.Wedding)
            .HasForeignKey(g => g.WeddingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(w => w.Tasks)
            .WithOne(t => t.Wedding)
            .HasForeignKey(t => t.WeddingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(w => w.CoordinatorId);
        builder.HasIndex(w => w.WeddingDate);
        builder.HasIndex(w => w.Status);
    }
}