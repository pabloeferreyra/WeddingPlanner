using WeddingPlanner.Core.Domain.Entities;

namespace WeddingPlanner.Application.Abstractions.Services;

public interface IVendorAssignmentService
{
    Task<IEnumerable<VendorAssignment>> GetAllVendorAssignmentsAsync();
    Task<VendorAssignment> GetVendorAssignmentByIdAsync(Guid vendorAssignmentId);
    Task<VendorAssignment> AddVendorAssignmentAsync(VendorAssignment vendorAssignment);
    Task<VendorAssignment> UpdateVendorAssignmentAsync(VendorAssignment vendorAssignment);
    Task DeleteVendorAssignmentAsync(Guid vendorAssignmentId);
    Task<bool> VendorAssignmentExistsAsync(Guid vendorAssignmentId);
}
