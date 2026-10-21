using WeddingPlanner.Application.Abstractions.Services;

namespace WeddingPlanner.Application.UseCases.DeleteWedding;

public sealed class DeleteWeddingHandler(IWeddingService weddingService)
{
    public Task HandleAsync(DeleteWeddingCommand command, CancellationToken cancellationToken = default) =>
        weddingService.DeleteAsync(command.WeddingId, cancellationToken);
}
