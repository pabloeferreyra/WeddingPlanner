using WeddingPlanner.Application.Abstractions.Services;
using WeddingPlanner.Core.Domain.Entities;

namespace WeddingPlanner.Application.UseCases.WeddingTables;

public sealed class GetWeddingSeatingHandler(IWeddingTableService service)
{
    public Task<WeddingSeatingSummary> HandleAsync(Guid weddingId) => service.GetByWeddingAsync(weddingId);
}

public sealed class GetWeddingTableHandler(IWeddingTableService service)
{
    public Task<WeddingTable?> HandleAsync(Guid id) => service.GetByIdAsync(id);
}

public sealed class AddWeddingTableHandler(IWeddingTableService service)
{
    public Task<WeddingTable> HandleAsync(WeddingTable table) => service.AddAsync(table);
}

public sealed class UpdateWeddingTableHandler(IWeddingTableService service)
{
    public Task<WeddingTable> HandleAsync(WeddingTable table) => service.UpdateAsync(table);
}

public sealed class DeleteWeddingTableHandler(IWeddingTableService service)
{
    public Task HandleAsync(Guid weddingId, Guid tableId) => service.DeleteAsync(weddingId, tableId);
}
