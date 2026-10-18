using Microsoft.Extensions.DependencyInjection;
using WeddingPlanner.Application.UseCases.CreateWedding;
using WeddingPlanner.Application.UseCases.GetUpcomingWeddings;

namespace WeddingPlanner.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<CreateWeddingHandler>();
        services.AddScoped<GetUpcomingWeddingsHandler>();
        
        return services;
    }
}
