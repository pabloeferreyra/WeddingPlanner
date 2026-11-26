using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using WeddingPlanner.Application.Abstractions.Persistence;
using WeddingPlanner.Application.Abstractions.Services;
using WeddingPlanner.Application.Implementations;
using WeddingPlanner.Core.Domain.Entities;
using WeddingPlanner.Infrastructure.Data;
using WeddingPlanner.Infrastructure.Repositories;

namespace WeddingPlanner.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            connectionString = new NpgsqlConnectionStringBuilder
            {
                Host = configuration["DB_HOST"]
                    ?? throw new InvalidOperationException("Environment variable 'DB_HOST' not found."),
                Port = configuration.GetValue<int?>("DB_PORT") ?? 5432,
                Database = configuration["DB_NAME"]
                    ?? throw new InvalidOperationException("Environment variable 'DB_NAME' not found."),
                Username = configuration["DB_USER"]
                    ?? throw new InvalidOperationException("Environment variable 'DB_USER' not found."),
                Password = configuration["DB_PASSWORD"]
                    ?? throw new InvalidOperationException("Environment variable 'DB_PASSWORD' not found.")
            }.ConnectionString;
        }

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString, provider =>
                provider.MigrationsAssembly("WeddingPlanner.Web")));
        services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IRepository<Vendor>, VendorRepository>();
        services.AddScoped<IAddressService, AddressService>();
        services.AddScoped<IChecklistItemService, ChecklistItemService>();
        services.AddScoped<IExpenseService, ExpenseService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<ITaskItemService, TaskItemService>();
        services.AddScoped<ITimelineService, TimelineService>();
        services.AddScoped<IVendorAssignmentService, VendorAssignmentService>();
        services.AddScoped<IVendorPaymentService, VendorPaymentService>();
        services.AddScoped<IVendorDocumentService, VendorDocumentService>();
        services.AddScoped<IVendorPaymentRepository, VendorPaymentRepository>();
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