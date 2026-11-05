using WeddingPlanner.Application.Abstractions.Services;
using WeddingPlanner.Core.Domain.Entities;
using WeddingPlanner.Core.Domain.Enums;

namespace WeddingPlanner.Application.UseCases.Vendors;

public sealed class GetAllVendorsHandler(IVendorService service)
{
    public Task<IEnumerable<Vendor>> HandleAsync() => service.GetAllVendorsAsync();
}

public sealed record GetVendorByIdQuery(Guid VendorId);

public sealed class GetVendorByIdHandler(IVendorService service)
{
    public Task<Vendor> HandleAsync(GetVendorByIdQuery query) => service.GetVendorByIdAsync(query.VendorId);
}

public sealed class AddVendorCommand
{
    public required string BusinessName { get; init; }
    public string? ContactPerson { get; init; }
    public string? Email { get; init; }
    public string? PhoneAreaCode { get; init; }
    public string? PhoneNumber { get; init; }
    public string? Website { get; init; }
    public required VendorServiceType ServiceType { get; init; }
    public required string Street { get; init; }
    public required string City { get; init; }
    public required string State { get; init; }
    public required string ZipCode { get; init; }
    public string? Description { get; init; }
    public decimal AverageRating { get; init; }
    public bool IsActive { get; init; } = true;
    public bool IsFavorite { get; init; }
    public string? FacebookUrl { get; init; }
    public string? InstagramUrl { get; init; }
}

public sealed class AddVendorHandler(IVendorService service)
{
    public Task<Vendor> HandleAsync(AddVendorCommand command) =>
        service.AddVendorAsync(new Vendor
        {
            BusinessName = command.BusinessName,
            ContactPerson = command.ContactPerson,
            Email = command.Email,
            Phone = command.PhoneAreaCode is null && command.PhoneNumber is null
                ? null
                : new PhoneNumber
                {
                    AreaCode = command.PhoneAreaCode,
                    Number = command.PhoneNumber
                },
            Website = command.Website,
            ServiceType = command.ServiceType,
            Address = new Address
            {
                Street = command.Street,
                City = command.City,
                State = command.State,
                ZipCode = command.ZipCode
            },
            Description = command.Description,
            AverageRating = command.AverageRating,
            IsActive = command.IsActive,
            IsFavorite = command.IsFavorite,
            FacebookUrl = command.FacebookUrl,
            InstagramUrl = command.InstagramUrl
        });
}

public sealed record UpdateVendorCommand(Vendor Vendor);

public sealed class UpdateVendorHandler(IVendorService service)
{
    public Task<Vendor> HandleAsync(UpdateVendorCommand command) => service.UpdateVendorAsync(command.Vendor);
}

public sealed record DeleteVendorCommand(Guid VendorId);

public sealed class DeleteVendorHandler(IVendorService service)
{
    public Task HandleAsync(DeleteVendorCommand command) => service.DeleteVendorAsync(command.VendorId);
}

public sealed record VendorExistsQuery(Guid VendorId);

public sealed class VendorExistsHandler(IVendorService service)
{
    public Task<bool> HandleAsync(VendorExistsQuery query) => service.VendorExistsAsync(query.VendorId);
}
