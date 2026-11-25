using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WeddingPlanner.Application.Abstractions.Persistence;
using WeddingPlanner.Core.Domain.Entities;
using WeddingPlanner.Infrastructure.Data;

namespace WeddingPlanner.Infrastructure.Repositories;

public sealed class VendorRepository(
    ApplicationDbContext context,
    ILogger<GenericRepository<Vendor>> logger)
    : GenericRepository<Vendor>(context, logger)
{
    public override async Task<Vendor> GetByIdAsync(Guid id)
    {
        _logger.LogDebug("Getting {Entity} with address with id {Id}", EntityName, id);

        var vendor = await _context.Vendors
            .Include(vendor => vendor.Address)
            .Include(vendor => vendor.Phone)
            .FirstOrDefaultAsync(vendor => vendor.Id == id);

        if (vendor is null)
        {
            _logger.LogWarning("{Entity} with id {Id} was not found", EntityName, id);
        }

        return vendor!;
    }

    public override async Task<IEnumerable<Vendor>> GetAllAsync()
    {
        _logger.LogDebug("Getting all {Entity} records with addresses", EntityName);

        var vendors = await _context.Vendors
            .Include(vendor => vendor.Address)
            .Include(vendor => vendor.Phone)
            .ToListAsync();

        _logger.LogDebug("Retrieved {Count} {Entity} records", vendors.Count, EntityName);

        return vendors;
    }
}
