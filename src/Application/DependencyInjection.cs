using Microsoft.Extensions.DependencyInjection;
using WeddingPlanner.Application.UseCases.CreateWedding;
using WeddingPlanner.Application.UseCases.DeleteWedding;
using WeddingPlanner.Application.UseCases.ExistsWedding;
using WeddingPlanner.Application.UseCases.GetUpcomingWeddings;
using WeddingPlanner.Application.UseCases.GetWeddingByDate;
using WeddingPlanner.Application.UseCases.GetWeddingById;
using WeddingPlanner.Application.UseCases.GetWeddingByTitle;
using WeddingPlanner.Application.UseCases.GetAllWeddings;
using WeddingPlanner.Application.UseCases.UpdateWedding;

namespace WeddingPlanner.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<CreateWeddingHandler>();
        services.AddScoped<GetUpcomingWeddingsHandler>();
        services.AddScoped<GetWeddingByIdHandler>();
        services.AddScoped<GetWeddingByDateHandler>();
        services.AddScoped<GetWeddingByTitleHandler>();
        services.AddScoped<GetAllWeddingsHandler>();
        services.AddScoped<UpdateWeddingHandler>();
        services.AddScoped<DeleteWeddingHandler>();
        services.AddScoped<ExistsWeddingHandler>();
        return services;
    }
}
