using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using WeddingPlanner.Core.Domain.Entities;
using WeddingPlanner.Infrastructure.Data;
using WeddingPlanner.Infrastructure.Repositories;

namespace WeddingPlanner.Tests.Unit;

public class VendorRepositoryTests
{
    [Fact]
    public async Task Vendor_queries_load_address()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var vendor = new Vendor
        {
            BusinessName = "Proveedor",
            Email = "proveedor@example.com",
            ServiceType = WeddingPlanner.Core.Domain.Enums.VendorServiceType.Other,
            Phone = new PhoneNumber { AreaCode = "11", Number = "5551234" },
            Address = new Address
            {
                Street = "Calle 1",
                City = "Buenos Aires",
                State = "Buenos Aires",
                ZipCode = "1000"
            }
        };

        await using var context = new ApplicationDbContext(options);
        context.Vendors.Add(vendor);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var repository = new VendorRepository(
            context,
            NullLogger<GenericRepository<Vendor>>.Instance);

        var byId = await repository.GetByIdAsync(vendor.Id);
        Assert.Equal("Buenos Aires", byId.Address.City);
        Assert.Equal("5551234", byId.Phone?.Number);

        context.ChangeTracker.Clear();
        var all = await repository.GetAllAsync();
        Assert.Equal("Buenos Aires", Assert.Single(all).Address.City);
    }

    [Fact]
    public async Task Update_of_tracked_vendor_saves_changes()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var vendor = new Vendor
        {
            BusinessName = "Proveedor",
            Email = "proveedor@example.com",
            ServiceType = WeddingPlanner.Core.Domain.Enums.VendorServiceType.Other,
            Address = new Address { Street = "Calle 1", City = "Rosario", State = "Santa Fe", ZipCode = "2000" }
        };

        await using var context = new ApplicationDbContext(options);
        context.Vendors.Add(vendor);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var repository = new VendorRepository(context, NullLogger<GenericRepository<Vendor>>.Instance);
        var loaded = await repository.GetByIdAsync(vendor.Id);
        loaded.BusinessName = "Nuevo nombre";
        loaded.Address.City = "Córdoba";
        await repository.UpdateAsync(loaded);

        context.ChangeTracker.Clear();
        var saved = await repository.GetByIdAsync(vendor.Id);
        Assert.Equal("Nuevo nombre", saved.BusinessName);
        Assert.Equal("Córdoba", saved.Address.City);
    }
}
