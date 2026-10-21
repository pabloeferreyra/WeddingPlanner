using WeddingPlanner.Application.Abstractions.Services;
using WeddingPlanner.Core.Domain.Entities;

namespace WeddingPlanner.Application.UseCases.VendorAssignments;

public sealed class GetAllVendorAssignmentsHandler(IVendorAssignmentService service)
{
    public Task<IEnumerable<VendorAssignment>> HandleAsync() => service.GetAllVendorAssignmentsAsync();
}

public sealed record GetVendorAssignmentByIdQuery(Guid VendorAssignmentId);

public sealed class GetVendorAssignmentByIdHandler(IVendorAssignmentService service)
{
    public Task<VendorAssignment> HandleAsync(GetVendorAssignmentByIdQuery query) =>
        service.GetVendorAssignmentByIdAsync(query.VendorAssignmentId);
}

public sealed record AddVendorAssignmentCommand(VendorAssignment VendorAssignment);

public sealed class AddVendorAssignmentHandler(IVendorAssignmentService service)
{
    public Task<VendorAssignment> HandleAsync(AddVendorAssignmentCommand command) =>
        service.AddVendorAssignmentAsync(command.VendorAssignment);
}

public sealed record UpdateVendorAssignmentCommand(VendorAssignment VendorAssignment);

public sealed class UpdateVendorAssignmentHandler(IVendorAssignmentService service)
{
    public Task<VendorAssignment> HandleAsync(UpdateVendorAssignmentCommand command) =>
        service.UpdateVendorAssignmentAsync(command.VendorAssignment);
}

public sealed record DeleteVendorAssignmentCommand(Guid VendorAssignmentId);

public sealed class DeleteVendorAssignmentHandler(IVendorAssignmentService service)
{
    public Task HandleAsync(DeleteVendorAssignmentCommand command) =>
        service.DeleteVendorAssignmentAsync(command.VendorAssignmentId);
}

public sealed record VendorAssignmentExistsQuery(Guid VendorAssignmentId);

public sealed class VendorAssignmentExistsHandler(IVendorAssignmentService service)
{
    public Task<bool> HandleAsync(VendorAssignmentExistsQuery query) =>
        service.VendorAssignmentExistsAsync(query.VendorAssignmentId);
}
