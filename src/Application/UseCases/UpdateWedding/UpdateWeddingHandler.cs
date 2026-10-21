using WeddingPlanner.Application.Abstractions.Services;

namespace WeddingPlanner.Application.UseCases.UpdateWedding;

public sealed class UpdateWeddingHandler(IWeddingService weddingService)
{
    public Task HandleAsync(UpdateWeddingCommand command, CancellationToken cancellationToken = default) =>
        weddingService.UpdateAsync(command.Wedding, cancellationToken);
}
