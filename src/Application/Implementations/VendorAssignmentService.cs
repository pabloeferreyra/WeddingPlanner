using WeddingPlanner.Application.Abstractions.Persistence;
using WeddingPlanner.Application.Abstractions.Services;
using WeddingPlanner.Core.Domain.Entities;

namespace WeddingPlanner.Application.Implementations;

public sealed class VendorAssignmentService(IRepository<VendorAssignment> repository) : IVendorAssignmentService
{
    public async Task<IEnumerable<VendorAssignment>> GetAllVendorAssignmentsAsync() => await repository.GetAllAsync();
    public async Task<VendorAssignment> GetVendorAssignmentByIdAsync(Guid vendorAssignmentId) => await repository.GetByIdAsync(vendorAssignmentId);
    public async Task<VendorAssignment> AddVendorAssignmentAsync(VendorAssignment vendorAssignment) => await repository.AddAsync(vendorAssignment);
    public async Task<VendorAssignment> UpdateVendorAssignmentAsync(VendorAssignment vendorAssignment) => await repository.UpdateAsync(vendorAssignment);
    public async Task DeleteVendorAssignmentAsync(Guid vendorAssignmentId) => await repository.DeleteAsync(vendorAssignmentId);
    public async Task<bool> VendorAssignmentExistsAsync(Guid vendorAssignmentId) => await repository.ExistsAsync(vendorAssignmentId);
}
