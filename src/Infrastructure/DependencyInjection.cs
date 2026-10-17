using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WeddingPlanner.Application.Abstractions.Persistence;
using WeddingPlanner.Application.Abstractions.Services;
using WeddingPlanner.Application.Implementations;
using WeddingPlanner.Infrastructure.Data;
using WeddingPlanner.Infrastructure.Repositories;

namespace WeddingPlanner.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));
        services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IAddressService, AddressService>();
        services.AddScoped<IChecklistItemService, ChecklistItemService>();
        services.AddScoped<IExpenseService, ExpenseService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<ITaskItemService, TaskItemService>();
        services.AddScoped<ITimelineService, TimelineService>();
        services.AddScoped<IVendorAssignmentService, VendorAssignmentService>();
        services.AddScoped<IVendorDocumentService, VendorDocumentService>();
        services.AddScoped<IWeddingRepository, WeddingRepository>();
        services.AddScoped<IGuestRepository, GuestRepository>();
        services.AddScoped<IGuestService, GuestService>();
        services.AddScoped<IWeddingService, WeddingService>();
        services.AddScoped<IBudgetRepository, BudgetRepository>();
        services.AddScoped<IBudgetService, BudgetService>();
        services.AddScoped<IVendorService, VendorService>();
        
        return services;
    }
}