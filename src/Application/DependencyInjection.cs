using Microsoft.Extensions.DependencyInjection;
using WeddingPlanner.Application.UseCases.Addresses;
using WeddingPlanner.Application.UseCases.Budgets;
using WeddingPlanner.Application.UseCases.ChecklistItems;
using WeddingPlanner.Application.UseCases.Expenses;
using WeddingPlanner.Application.UseCases.Guests;
using WeddingPlanner.Application.UseCases.Notifications;
using WeddingPlanner.Application.UseCases.TaskItems;
using WeddingPlanner.Application.UseCases.Timelines;
using WeddingPlanner.Application.UseCases.VendorAssignments;
using WeddingPlanner.Application.UseCases.VendorDocuments;
using WeddingPlanner.Application.UseCases.Vendors;
using WeddingPlanner.Application.UseCases.Weddings;

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

        services.AddScoped<GetAllAddressesHandler>();
        services.AddScoped<GetAddressByIdHandler>();
        services.AddScoped<AddAddressHandler>();
        services.AddScoped<UpdateAddressHandler>();
        services.AddScoped<DeleteAddressHandler>();
        services.AddScoped<AddressExistsHandler>();

        services.AddScoped<GetBudgetByWeddingHandler>();
        services.AddScoped<AddBudgetHandler>();
        services.AddScoped<UpdateBudgetHandler>();
        services.AddScoped<DeleteBudgetHandler>();
        services.AddScoped<GetAllBudgetsHandler>();
        services.AddScoped<GetBudgetByIdHandler>();

        services.AddScoped<GetAllChecklistItemsHandler>();
        services.AddScoped<GetChecklistItemByIdHandler>();
        services.AddScoped<AddChecklistItemHandler>();
        services.AddScoped<UpdateChecklistItemHandler>();
        services.AddScoped<DeleteChecklistItemHandler>();
        services.AddScoped<ChecklistItemExistsHandler>();

        services.AddScoped<GetAllExpensesHandler>();
        services.AddScoped<GetExpenseByIdHandler>();
        services.AddScoped<AddExpenseHandler>();
        services.AddScoped<UpdateExpenseHandler>();
        services.AddScoped<DeleteExpenseHandler>();
        services.AddScoped<ExpenseExistsHandler>();

        services.AddScoped<GetAllGuestsHandler>();
        services.AddScoped<GetGuestByIdHandler>();
        services.AddScoped<AddGuestHandler>();
        services.AddScoped<UpdateGuestHandler>();
        services.AddScoped<DeleteGuestHandler>();
        services.AddScoped<GuestExistsHandler>();
        services.AddScoped<GetGuestsByWeddingHandler>();
        services.AddScoped<GetGuestsByStatusHandler>();

        services.AddScoped<GetAllNotificationsHandler>();
        services.AddScoped<GetNotificationByIdHandler>();
        services.AddScoped<AddNotificationHandler>();
        services.AddScoped<UpdateNotificationHandler>();
        services.AddScoped<DeleteNotificationHandler>();
        services.AddScoped<NotificationExistsHandler>();

        services.AddScoped<GetAllTaskItemsHandler>();
        services.AddScoped<GetTaskItemByIdHandler>();
        services.AddScoped<AddTaskItemHandler>();
        services.AddScoped<UpdateTaskItemHandler>();
        services.AddScoped<DeleteTaskItemHandler>();
        services.AddScoped<TaskItemExistsHandler>();

        services.AddScoped<GetAllTimelinesHandler>();
        services.AddScoped<GetTimelineByIdHandler>();
        services.AddScoped<AddTimelineHandler>();
        services.AddScoped<UpdateTimelineHandler>();
        services.AddScoped<DeleteTimelineHandler>();
        services.AddScoped<TimelineExistsHandler>();

        services.AddScoped<GetAllVendorAssignmentsHandler>();
        services.AddScoped<GetVendorAssignmentByIdHandler>();
        services.AddScoped<AddVendorAssignmentHandler>();
        services.AddScoped<UpdateVendorAssignmentHandler>();
        services.AddScoped<DeleteVendorAssignmentHandler>();
        services.AddScoped<VendorAssignmentExistsHandler>();

        services.AddScoped<GetAllVendorDocumentsHandler>();
        services.AddScoped<GetVendorDocumentByIdHandler>();
        services.AddScoped<AddVendorDocumentHandler>();
        services.AddScoped<UpdateVendorDocumentHandler>();
        services.AddScoped<DeleteVendorDocumentHandler>();
        services.AddScoped<VendorDocumentExistsHandler>();

        services.AddScoped<GetAllVendorsHandler>();
        services.AddScoped<GetVendorByIdHandler>();
        services.AddScoped<AddVendorHandler>();
        services.AddScoped<UpdateVendorHandler>();
        services.AddScoped<DeleteVendorHandler>();
        services.AddScoped<VendorExistsHandler>();

        return services;
    }
}
