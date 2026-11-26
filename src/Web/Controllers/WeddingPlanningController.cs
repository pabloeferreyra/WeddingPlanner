using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using WeddingPlanner.Application.UseCases.Budgets;
using WeddingPlanner.Application.UseCases.ChecklistItems;
using WeddingPlanner.Application.UseCases.Expenses;
using WeddingPlanner.Application.UseCases.Guests;
using WeddingPlanner.Application.UseCases.TaskItems;
using WeddingPlanner.Application.UseCases.Timelines;
using WeddingPlanner.Application.UseCases.VendorAssignments;
using WeddingPlanner.Application.UseCases.VendorDocuments;
using WeddingPlanner.Application.UseCases.VendorPayments;
using WeddingPlanner.Application.UseCases.Vendors;
using WeddingPlanner.Application.UseCases.Weddings;
using WeddingPlanner.Application.UseCases.WeddingTables;
using WeddingPlanner.Core.Domain.Entities;
using WeddingPlanner.Core.Domain.Enums;
using WeddingPlanner.Core.Domain.Services;
using WeddingPlanner.Web.Models;
using TaskStatus = WeddingPlanner.Core.Domain.Enums.TaskStatus;

namespace WeddingPlanner.Web.Controllers;

[Route("Weddings/{weddingId:guid}/Planning")]
public sealed class WeddingPlanningController(
    GetWeddingByIdHandler getWeddingById,
    GetGuestsByWeddingHandler getGuestsByWedding,
    GetGuestByIdHandler getGuestById,
    AddGuestHandler addGuest,
    UpdateGuestHandler updateGuest,
    DeleteGuestHandler deleteGuest,
    GetBudgetByWeddingHandler getBudgetByWedding,
    AddBudgetHandler addBudget,
    UpdateBudgetHandler updateBudget,
    GetAllExpensesHandler getAllExpenses,
    GetExpenseByIdHandler getExpenseById,
    AddExpenseHandler addExpense,
    UpdateExpenseHandler updateExpense,
    DeleteExpenseHandler deleteExpense,
    GetAllTaskItemsHandler getAllTasks,
    GetTaskItemByIdHandler getTaskById,
    AddTaskItemHandler addTask,
    UpdateTaskItemHandler updateTask,
    DeleteTaskItemHandler deleteTask,
    GetAllChecklistItemsHandler getAllChecklistItems,
    GetChecklistItemByIdHandler getChecklistItemById,
    AddChecklistItemHandler addChecklistItem,
    UpdateChecklistItemHandler updateChecklistItem,
    DeleteChecklistItemHandler deleteChecklistItem,
    GetAllTimelinesHandler getAllTimelines,
    GetTimelineByIdHandler getTimelineById,
    AddTimelineHandler addTimeline,
    UpdateTimelineHandler updateTimeline,
    DeleteTimelineHandler deleteTimeline,
    GetAllVendorAssignmentsHandler getAllVendorAssignments,
    GetVendorAssignmentByIdHandler getVendorAssignmentById,
    AddVendorAssignmentHandler addVendorAssignment,
    UpdateVendorAssignmentHandler updateVendorAssignment,
    DeleteVendorAssignmentHandler deleteVendorAssignment,
    GetVendorPaymentsByWeddingHandler getVendorPaymentsByWedding,
    GetVendorPaymentByIdHandler getVendorPaymentById,
    AddVendorPaymentHandler addVendorPayment,
    UpdateVendorPaymentHandler updateVendorPayment,
    DeleteVendorPaymentHandler deleteVendorPayment,
    GetAllVendorDocumentsHandler getAllVendorDocuments,
    GetAllVendorsHandler getAllVendors,
    GetWeddingSeatingHandler getWeddingSeating,
    GetWeddingTableHandler getWeddingTable,
    AddWeddingTableHandler addWeddingTable,
    UpdateWeddingTableHandler updateWeddingTable,
    DeleteWeddingTableHandler deleteWeddingTable)
    : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(Guid weddingId, CancellationToken cancellationToken)
    {
        var wedding = await getWeddingById.HandleAsync(
            new GetWeddingByIdQuery(weddingId),
            cancellationToken);
        if (wedding is null)
        {
            return NotFound();
        }

        var budget = await getBudgetByWedding.HandleAsync(
            new GetBudgetByWeddingQuery(weddingId),
            cancellationToken);
        var tasks = (await getAllTasks.HandleAsync())
            .Where(task => task.WeddingId == weddingId)
            .OrderBy(task => task.DueDate)
            .ToList();
        var taskIds = tasks.Select(task => task.Id).ToHashSet();
        var checklists = (await getAllChecklistItems.HandleAsync())
            .Where(item => taskIds.Contains(item.TaskId))
            .OrderBy(item => item.CreatedAt)
            .ToList();
        var assignments = (await getAllVendorAssignments.HandleAsync())
            .Where(assignment => assignment.WeddingId == weddingId)
            .ToList();
        var vendorPayments = await getVendorPaymentsByWedding.HandleAsync(
            new GetVendorPaymentsByWeddingQuery(weddingId));
        var guests = await getGuestsByWedding.HandleAsync(
            new GetGuestsByWeddingQuery(weddingId),
            cancellationToken);
        var attendance = GuestAttendanceCalculator.Calculate(guests);
        var timelines = (await getAllTimelines.HandleAsync())
            .Where(timeline => timeline.WeddingId == weddingId)
            .OrderBy(timeline => timeline.ScheduledDate)
            .ToList();

        var completedProgressItems = tasks.Count(task =>
            task.Status == TaskStatus.Completed)
            + checklists.Count(item => item.IsCompleted)
            + timelines.Count(item => item.IsCompleted);
        var totalProgressItems = tasks.Count + checklists.Count + timelines.Count;

        ViewData["BudgetRemaining"] = budget?.GetRemainingBudget();
        ViewData["CompletedTaskCount"] = tasks.Count(task =>
            task.Status == TaskStatus.Completed);
        ViewData["CompletedProgressItems"] = completedProgressItems;
        ViewData["TotalProgressItems"] = totalProgressItems;
        ViewData["PlanningProgressPercent"] = totalProgressItems == 0
            ? null
            : (int?)Math.Round(completedProgressItems * 100d / totalProgressItems);

        return View(new WeddingPlanningViewModel
        {
            Wedding = wedding,
            Budget = budget,
            Guests = guests,
            Seating = await getWeddingSeating.HandleAsync(weddingId),
            ConfirmedMainGuestCount = attendance.ConfirmedMainGuestCount,
            ConfirmedCompanionCount = attendance.ConfirmedCompanionCount,
            ConfirmedAttendeeCount = attendance.ConfirmedAttendeeCount,
            Expenses = budget is null
                ? []
                : (await getAllExpenses.HandleAsync())
                    .Where(expense => expense.BudgetId == budget.Id)
                    .OrderByDescending(expense => expense.ExpenseDate)
                    .ToList(),
            Tasks = tasks,
            ChecklistItems = checklists,
            Timelines = timelines,
            VendorAssignments = assignments,
            Vendors = (await getAllVendors.HandleAsync()).ToList(),
            VendorPayments = vendorPayments
        });
    }

    [HttpGet("Guests/Create")]
    public async Task<IActionResult> CreateGuest(Guid weddingId)
    {
        if (!await WeddingExists(weddingId))
        {
            return NotFound();
        }

        return await GuestForm(new GuestFormViewModel { WeddingId = weddingId });
    }

    [HttpGet("Guests/{id:guid}/Edit")]
    public async Task<IActionResult> EditGuest(Guid weddingId, Guid id)
    {
        var guest = await getGuestById.HandleAsync(new GetGuestByIdQuery(id));
        if (guest is null || guest.WeddingId != weddingId)
        {
            return NotFound();
        }

        return await GuestForm(ToFormModel(guest));
    }

    [HttpPost("Guests/Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateGuest(Guid weddingId, GuestFormViewModel model)
    {
        var wedding = await getWeddingById.HandleAsync(new GetWeddingByIdQuery(weddingId));
        if (wedding is null)
        {
            return NotFound();
        }

        model.Id = Guid.Empty;
        model.WeddingId = weddingId;
        if (!ModelState.IsValid)
        {
            return await GuestForm(model);
        }

        try
        {
            await addGuest.HandleAsync(new AddGuestCommand(ToEntity(model, wedding)));
        }
        catch (ValidationException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return await GuestForm(model);
        }
        return RedirectToAction(nameof(Index), new { weddingId });
    }

    [HttpPost("Guests/{id:guid}/Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditGuest(Guid weddingId, Guid id, GuestFormViewModel model)
    {
        var guest = await getGuestById.HandleAsync(new GetGuestByIdQuery(id));
        if (guest is null || guest.WeddingId != weddingId)
        {
            return NotFound();
        }

        model.Id = id;
        model.WeddingId = weddingId;
        if (!ModelState.IsValid)
        {
            return await GuestForm(model);
        }

        CopyToEntity(model, guest);
        try
        {
            await updateGuest.HandleAsync(new UpdateGuestCommand(guest));
        }
        catch (ValidationException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return await GuestForm(model);
        }
        return RedirectToAction(nameof(Index), new { weddingId });
    }

    [HttpGet("Tables/Create")]
    public async Task<IActionResult> CreateWeddingTable(Guid weddingId)
    {
        if (!await WeddingExists(weddingId)) return NotFound();
        return PartialView("Forms/_WeddingTableForm", new WeddingTableFormViewModel { WeddingId = weddingId });
    }

    [HttpGet("Tables/{id:guid}/Edit")]
    public async Task<IActionResult> EditWeddingTable(Guid weddingId, Guid id)
    {
        var table = await getWeddingTable.HandleAsync(id);
        if (table is null || table.WeddingId != weddingId) return NotFound();
        return PartialView("Forms/_WeddingTableForm", new WeddingTableFormViewModel
        {
            Id = id, WeddingId = weddingId, Name = table.Name, Capacity = table.Capacity, Notes = table.Notes
        });
    }

    [HttpPost("Tables/Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateWeddingTable(Guid weddingId, WeddingTableFormViewModel model)
    {
        if (!await WeddingExists(weddingId)) return NotFound();
        model.Id = Guid.Empty;
        model.WeddingId = weddingId;
        if (!ModelState.IsValid) return PartialView("Forms/_WeddingTableForm", model);
        try
        {
            await addWeddingTable.HandleAsync(new WeddingTable
            {
                WeddingId = weddingId, Name = model.Name, Capacity = model.Capacity, Notes = model.Notes
            });
        }
        catch (ValidationException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return PartialView("Forms/_WeddingTableForm", model);
        }
        return RedirectToAction(nameof(Index), new { weddingId });
    }

    [HttpPost("Tables/{id:guid}/Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditWeddingTable(Guid weddingId, Guid id, WeddingTableFormViewModel model)
    {
        var table = await getWeddingTable.HandleAsync(id);
        if (table is null || table.WeddingId != weddingId) return NotFound();
        model.Id = id;
        model.WeddingId = weddingId;
        if (!ModelState.IsValid) return PartialView("Forms/_WeddingTableForm", model);
        table.Name = model.Name;
        table.Capacity = model.Capacity;
        table.Notes = model.Notes;
        try
        {
            await updateWeddingTable.HandleAsync(table);
        }
        catch (ValidationException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return PartialView("Forms/_WeddingTableForm", model);
        }
        return RedirectToAction(nameof(Index), new { weddingId });
    }

    [HttpPost("Tables/{id:guid}/Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteWeddingTable(Guid weddingId, Guid id)
    {
        var table = await getWeddingTable.HandleAsync(id);
        if (table is null || table.WeddingId != weddingId) return NotFound();
        try
        {
            await deleteWeddingTable.HandleAsync(weddingId, id);
        }
        catch (ValidationException exception)
        {
            TempData["ModuleMessage"] = exception.Message;
        }
        return RedirectToAction(nameof(Index), new { weddingId });
    }

    [HttpPost("Guests/{id:guid}/Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteGuest(Guid weddingId, Guid id)
    {
        var guest = await getGuestById.HandleAsync(new GetGuestByIdQuery(id));
        if (guest is null || guest.WeddingId != weddingId)
        {
            return NotFound();
        }

        await deleteGuest.HandleAsync(new DeleteGuestCommand(id));
        return RedirectToAction(nameof(Index), new { weddingId });
    }

    [HttpGet("Budget/Create")]
    public async Task<IActionResult> CreateBudget(Guid weddingId)
    {
        if (!await WeddingExists(weddingId))
        {
            return NotFound();
        }

        var current = await getBudgetByWedding.HandleAsync(new GetBudgetByWeddingQuery(weddingId));
        if (current is not null)
        {
            return NotFound();
        }

        return PartialView("Forms/_BudgetForm", new BudgetFormViewModel { WeddingId = weddingId });
    }

    [HttpGet("Budget/{id:guid}/Edit")]
    public async Task<IActionResult> EditBudget(Guid weddingId, Guid id)
    {
        var budget = await getBudgetByWedding.HandleAsync(new GetBudgetByWeddingQuery(weddingId));
        if (budget is null || budget.Id != id)
        {
            return NotFound();
        }

        return PartialView("Forms/_BudgetForm", ToFormModel(budget));
    }

    [HttpPost("Budget/Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateBudget(Guid weddingId, BudgetFormViewModel model)
    {
        if (!await WeddingExists(weddingId))
        {
            return NotFound();
        }

        model.Id = Guid.Empty;
        model.WeddingId = weddingId;
        if (await getBudgetByWedding.HandleAsync(new GetBudgetByWeddingQuery(weddingId)) is not null)
        {
            ModelState.AddModelError(string.Empty, "Esta celebración ya tiene un presupuesto.");
        }

        if (!ModelState.IsValid)
        {
            return PartialView("Forms/_BudgetForm", model);
        }

        try
        {
            await addBudget.HandleAsync(new AddBudgetCommand(new Budget
            {
                WeddingId = weddingId,
                TotalAmount = model.TotalAmount,
                CategoryAllocations = model.CategoryAllocations
                    .Where(allocation => allocation.Value.HasValue)
                    .ToDictionary(allocation => allocation.Key, allocation => allocation.Value!.Value)
            }));
        }
        catch (ValidationException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return PartialView("Forms/_BudgetForm", model);
        }

        return RedirectToAction(nameof(Index), new { weddingId });
    }

    [HttpPost("Budget/{id:guid}/Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditBudget(Guid weddingId, Guid id, BudgetFormViewModel model)
    {
        var budget = await getBudgetByWedding.HandleAsync(new GetBudgetByWeddingQuery(weddingId));
        if (budget is null || budget.Id != id)
        {
            return NotFound();
        }

        model.Id = id;
        model.WeddingId = weddingId;
        if (!ModelState.IsValid)
        {
            return PartialView("Forms/_BudgetForm", model);
        }

        budget.TotalAmount = model.TotalAmount;
        budget.CategoryAllocations = model.CategoryAllocations
            .Where(allocation => allocation.Value.HasValue)
            .ToDictionary(allocation => allocation.Key, allocation => allocation.Value!.Value);
        try
        {
            await updateBudget.HandleAsync(new UpdateBudgetCommand(budget));
        }
        catch (ValidationException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return PartialView("Forms/_BudgetForm", model);
        }

        return RedirectToAction(nameof(Index), new { weddingId });
    }

    [HttpGet("Expenses/Create")]
    public async Task<IActionResult> CreateExpense(Guid weddingId, CancellationToken cancellationToken)
    {
        if (!await WeddingExists(weddingId))
        {
            return NotFound();
        }

        var budget = await getBudgetByWedding.HandleAsync(
            new GetBudgetByWeddingQuery(weddingId),
            cancellationToken);
        if (budget is null)
        {
            return NotFound();
        }

        return PartialView("Forms/_ExpenseForm", new ExpenseFormViewModel
        {
            WeddingId = weddingId,
            BudgetId = budget.Id
        });
    }

    [HttpGet("Expenses/{id:guid}/Edit")]
    public async Task<IActionResult> EditExpense(Guid weddingId, Guid id, CancellationToken cancellationToken)
    {
        var budget = await getBudgetByWedding.HandleAsync(
            new GetBudgetByWeddingQuery(weddingId),
            cancellationToken);
        var expense = await getExpenseById.HandleAsync(new GetExpenseByIdQuery(id));
        if (budget is null || expense is null || expense.BudgetId != budget.Id)
        {
            return NotFound();
        }

        var model = ToFormModel(expense);
        model.WeddingId = weddingId;
        return PartialView("Forms/_ExpenseForm", model);
    }

    [HttpPost("Expenses/Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateExpense(
        Guid weddingId,
        ExpenseFormViewModel model,
        CancellationToken cancellationToken)
    {
        var budget = await getBudgetByWedding.HandleAsync(
            new GetBudgetByWeddingQuery(weddingId),
            cancellationToken);
        if (budget is null)
        {
            return NotFound();
        }

        model.Id = Guid.Empty;
        model.BudgetId = budget.Id;
        model.WeddingId = weddingId;
        if (!ModelState.IsValid)
        {
            return PartialView("Forms/_ExpenseForm", model);
        }

        try
        {
            await addExpense.HandleAsync(new AddExpenseCommand(ToEntity(model)));
        }
        catch (ValidationException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return PartialView("Forms/_ExpenseForm", model);
        }

        return RedirectToAction(nameof(Index), new { weddingId });
    }

    [HttpPost("Expenses/{id:guid}/Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditExpense(
        Guid weddingId,
        Guid id,
        ExpenseFormViewModel model,
        CancellationToken cancellationToken)
    {
        var budget = await getBudgetByWedding.HandleAsync(
            new GetBudgetByWeddingQuery(weddingId),
            cancellationToken);
        var expense = await getExpenseById.HandleAsync(new GetExpenseByIdQuery(id));
        if (budget is null || expense is null || expense.BudgetId != budget.Id)
        {
            return NotFound();
        }

        model.Id = id;
        model.BudgetId = budget.Id;
        model.WeddingId = weddingId;
        if (!ModelState.IsValid)
        {
            return PartialView("Forms/_ExpenseForm", model);
        }

        CopyToEntity(model, expense);
        try
        {
            await updateExpense.HandleAsync(new UpdateExpenseCommand(expense));
        }
        catch (ValidationException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return PartialView("Forms/_ExpenseForm", model);
        }

        return RedirectToAction(nameof(Index), new { weddingId });
    }

    [HttpPost("Expenses/{id:guid}/Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteExpense(
        Guid weddingId,
        Guid id,
        CancellationToken cancellationToken)
    {
        var budget = await getBudgetByWedding.HandleAsync(
            new GetBudgetByWeddingQuery(weddingId),
            cancellationToken);
        var expense = await getExpenseById.HandleAsync(new GetExpenseByIdQuery(id));
        if (budget is null || expense is null || expense.BudgetId != budget.Id)
        {
            return NotFound();
        }

        await deleteExpense.HandleAsync(new DeleteExpenseCommand(id));
        return RedirectToAction(nameof(Index), new { weddingId });
    }

    [HttpGet("Tasks/Create")]
    public async Task<IActionResult> CreateTask(Guid weddingId)
    {
        if (!await WeddingExists(weddingId))
        {
            return NotFound();
        }

        return PartialView("Forms/_TaskForm", new TaskItemFormViewModel { WeddingId = weddingId });
    }

    [HttpGet("Tasks/{id:guid}/Edit")]
    public async Task<IActionResult> EditTask(Guid weddingId, Guid id)
    {
        var task = await getTaskById.HandleAsync(new GetTaskItemByIdQuery(id));
        if (task is null || task.WeddingId != weddingId)
        {
            return NotFound();
        }

        return PartialView("Forms/_TaskForm", ToFormModel(task));
    }

    [HttpPost("Tasks/Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateTask(Guid weddingId, TaskItemFormViewModel model)
    {
        if (!await WeddingExists(weddingId))
        {
            return NotFound();
        }

        model.Id = Guid.Empty;
        model.WeddingId = weddingId;
        if (!ModelState.IsValid)
        {
            return PartialView("Forms/_TaskForm", model);
        }

        await addTask.HandleAsync(new AddTaskItemCommand(ToEntity(model)));
        return RedirectToAction(nameof(Index), new { weddingId });
    }

    [HttpPost("Tasks/{id:guid}/Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditTask(Guid weddingId, Guid id, TaskItemFormViewModel model)
    {
        var task = await getTaskById.HandleAsync(new GetTaskItemByIdQuery(id));
        if (task is null || task.WeddingId != weddingId)
        {
            return NotFound();
        }

        model.Id = id;
        model.WeddingId = weddingId;
        if (!ModelState.IsValid)
        {
            return PartialView("Forms/_TaskForm", model);
        }

        CopyToEntity(model, task);
        await updateTask.HandleAsync(new UpdateTaskItemCommand(task));
        return RedirectToAction(nameof(Index), new { weddingId });
    }

    [HttpPost("Tasks/{id:guid}/Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteTask(Guid weddingId, Guid id)
    {
        var task = await getTaskById.HandleAsync(new GetTaskItemByIdQuery(id));
        if (task is null || task.WeddingId != weddingId)
        {
            return NotFound();
        }

        var checklist = (await getAllChecklistItems.HandleAsync())
            .Where(item => item.TaskId == id)
            .ToList();
        if (checklist.Count > 0)
        {
            TempData["ModuleMessage"] = "Elimina primero los elementos del checklist para poder eliminar esta tarea.";
            return RedirectToAction(nameof(Index), new { weddingId });
        }

        await deleteTask.HandleAsync(new DeleteTaskItemCommand(id));
        return RedirectToAction(nameof(Index), new { weddingId });
    }

    [HttpGet("Tasks/{taskId:guid}/Checklist/Create")]
    public async Task<IActionResult> CreateChecklistItem(Guid weddingId, Guid taskId)
    {
        if (!await TaskBelongsToWedding(taskId, weddingId))
        {
            return NotFound();
        }

        return PartialView("Forms/_ChecklistItemForm", new ChecklistItemFormViewModel
        {
            WeddingId = weddingId,
            TaskId = taskId
        });
    }

    [HttpGet("Tasks/{taskId:guid}/Checklist/{id:guid}/Edit")]
    public async Task<IActionResult> EditChecklistItem(Guid weddingId, Guid taskId, Guid id)
    {
        var item = await getChecklistItemById.HandleAsync(new GetChecklistItemByIdQuery(id));
        if (item is null || item.TaskId != taskId || !await TaskBelongsToWedding(taskId, weddingId))
        {
            return NotFound();
        }

        return PartialView("Forms/_ChecklistItemForm", ToFormModel(item, weddingId));
    }

    [HttpPost("Tasks/{taskId:guid}/Checklist/Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateChecklistItem(
        Guid weddingId,
        Guid taskId,
        ChecklistItemFormViewModel model)
    {
        if (!await TaskBelongsToWedding(taskId, weddingId))
        {
            return NotFound();
        }

        model.Id = Guid.Empty;
        model.WeddingId = weddingId;
        model.TaskId = taskId;
        if (!ModelState.IsValid)
        {
            return PartialView("Forms/_ChecklistItemForm", model);
        }

        await addChecklistItem.HandleAsync(new AddChecklistItemCommand(new ChecklistItem
        {
            TaskId = taskId,
            Description = model.Description,
            IsCompleted = model.IsCompleted,
            CompletedAt = model.IsCompleted ? DateTime.Now : null
        }));
        return RedirectToAction(nameof(Index), new { weddingId });
    }

    [HttpPost("Tasks/{taskId:guid}/Checklist/{id:guid}/Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditChecklistItem(
        Guid weddingId,
        Guid taskId,
        Guid id,
        ChecklistItemFormViewModel model)
    {
        var item = await getChecklistItemById.HandleAsync(new GetChecklistItemByIdQuery(id));
        if (item is null || item.TaskId != taskId || !await TaskBelongsToWedding(taskId, weddingId))
        {
            return NotFound();
        }

        model.Id = id;
        model.WeddingId = weddingId;
        model.TaskId = taskId;
        if (!ModelState.IsValid)
        {
            return PartialView("Forms/_ChecklistItemForm", model);
        }

        item.Description = model.Description;
        item.IsCompleted = model.IsCompleted;
        item.CompletedAt = model.IsCompleted ? item.CompletedAt ?? DateTime.Now : null;
        await updateChecklistItem.HandleAsync(new UpdateChecklistItemCommand(item));
        return RedirectToAction(nameof(Index), new { weddingId });
    }

    [HttpPost("Tasks/{taskId:guid}/Checklist/{id:guid}/Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteChecklistItem(Guid weddingId, Guid taskId, Guid id)
    {
        var item = await getChecklistItemById.HandleAsync(new GetChecklistItemByIdQuery(id));
        if (item is null || item.TaskId != taskId || !await TaskBelongsToWedding(taskId, weddingId))
        {
            return NotFound();
        }

        await deleteChecklistItem.HandleAsync(new DeleteChecklistItemCommand(id));
        return RedirectToAction(nameof(Index), new { weddingId });
    }

    [HttpGet("Timeline/Create")]
    public async Task<IActionResult> CreateTimeline(Guid weddingId)
    {
        if (!await WeddingExists(weddingId))
        {
            return NotFound();
        }

        return PartialView("Forms/_TimelineForm", new TimelineFormViewModel { WeddingId = weddingId });
    }

    [HttpGet("Timeline/{id:guid}/Edit")]
    public async Task<IActionResult> EditTimeline(Guid weddingId, Guid id)
    {
        var timeline = await getTimelineById.HandleAsync(new GetTimelineByIdQuery(id));
        if (timeline is null || timeline.WeddingId != weddingId)
        {
            return NotFound();
        }

        return PartialView("Forms/_TimelineForm", ToFormModel(timeline));
    }

    [HttpPost("Timeline/Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateTimeline(Guid weddingId, TimelineFormViewModel model)
    {
        if (!await WeddingExists(weddingId))
        {
            return NotFound();
        }

        model.Id = Guid.Empty;
        model.WeddingId = weddingId;
        if (!ModelState.IsValid)
        {
            return PartialView("Forms/_TimelineForm", model);
        }

        await addTimeline.HandleAsync(new AddTimelineCommand(ToEntity(model)));
        return RedirectToAction(nameof(Index), new { weddingId });
    }

    [HttpPost("Timeline/{id:guid}/Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditTimeline(Guid weddingId, Guid id, TimelineFormViewModel model)
    {
        var timeline = await getTimelineById.HandleAsync(new GetTimelineByIdQuery(id));
        if (timeline is null || timeline.WeddingId != weddingId)
        {
            return NotFound();
        }

        model.Id = id;
        model.WeddingId = weddingId;
        if (!ModelState.IsValid)
        {
            return PartialView("Forms/_TimelineForm", model);
        }

        CopyToEntity(model, timeline);
        await updateTimeline.HandleAsync(new UpdateTimelineCommand(timeline));
        return RedirectToAction(nameof(Index), new { weddingId });
    }

    [HttpPost("Timeline/{id:guid}/Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteTimeline(Guid weddingId, Guid id)
    {
        var timeline = await getTimelineById.HandleAsync(new GetTimelineByIdQuery(id));
        if (timeline is null || timeline.WeddingId != weddingId)
        {
            return NotFound();
        }

        await deleteTimeline.HandleAsync(new DeleteTimelineCommand(id));
        return RedirectToAction(nameof(Index), new { weddingId });
    }

    [HttpGet("Vendors/Create")]
    public async Task<IActionResult> CreateVendorAssignment(Guid weddingId)
    {
        if (!await WeddingExists(weddingId))
        {
            return NotFound();
        }

        return await VendorAssignmentForm(new VendorAssignmentFormViewModel
        {
            WeddingId = weddingId
        });
    }

    [HttpGet("Vendors/{id:guid}/Edit")]
    public async Task<IActionResult> EditVendorAssignment(Guid weddingId, Guid id)
    {
        var assignment = await getVendorAssignmentById.HandleAsync(
            new GetVendorAssignmentByIdQuery(id));
        if (assignment is null || assignment.WeddingId != weddingId)
        {
            return NotFound();
        }

        return await VendorAssignmentForm(ToFormModel(assignment));
    }

    [HttpPost("Vendors/Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateVendorAssignment(
        Guid weddingId,
        VendorAssignmentFormViewModel model)
    {
        if (!await WeddingExists(weddingId))
        {
            return NotFound();
        }

        var vendor = model.VendorId == Guid.Empty
            ? null
            : (await getAllVendors.HandleAsync()).FirstOrDefault(candidate => candidate.Id == model.VendorId);
        if (vendor is null)
        {
            ModelState.AddModelError(nameof(model.VendorId), "Selecciona un proveedor válido.");
        }

        model.Id = Guid.Empty;
        model.WeddingId = weddingId;
        if (!ModelState.IsValid)
        {
            return await VendorAssignmentForm(model);
        }

        try
        {
            await addVendorAssignment.HandleAsync(new AddVendorAssignmentCommand(ToEntity(model, vendor!)));
        }
        catch (ValidationException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return await VendorAssignmentForm(model);
        }

        return RedirectToAction(nameof(Index), new { weddingId });
    }

    [HttpPost("Vendors/{id:guid}/Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditVendorAssignment(
        Guid weddingId,
        Guid id,
        VendorAssignmentFormViewModel model)
    {
        var assignment = await getVendorAssignmentById.HandleAsync(
            new GetVendorAssignmentByIdQuery(id));
        if (assignment is null || assignment.WeddingId != weddingId)
        {
            return NotFound();
        }

        var vendor = model.VendorId == Guid.Empty
            ? null
            : (await getAllVendors.HandleAsync()).FirstOrDefault(candidate => candidate.Id == model.VendorId);
        if (vendor is null)
        {
            ModelState.AddModelError(nameof(model.VendorId), "Selecciona un proveedor válido.");
        }
        else if (vendor.Id != assignment.VendorId
                 && (await getAllVendorDocuments.HandleAsync()).Any(document => document.VendorAssignmentId == id))
        {
            ModelState.AddModelError(nameof(model.VendorId), "No puedes cambiar el proveedor mientras haya documentos vinculados.");
        }

        model.Id = id;
        model.WeddingId = weddingId;
        if (!ModelState.IsValid)
        {
            return await VendorAssignmentForm(model);
        }

        CopyToEntity(model, vendor!, assignment);
        try
        {
            await updateVendorAssignment.HandleAsync(new UpdateVendorAssignmentCommand(assignment));
        }
        catch (ValidationException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return await VendorAssignmentForm(model);
        }

        return RedirectToAction(nameof(Index), new { weddingId });
    }

    [HttpPost("Vendors/{id:guid}/Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteVendorAssignment(Guid weddingId, Guid id)
    {
        var assignment = await getVendorAssignmentById.HandleAsync(
            new GetVendorAssignmentByIdQuery(id));
        if (assignment is null || assignment.WeddingId != weddingId)
        {
            return NotFound();
        }

        if ((await getVendorPaymentsByWedding.HandleAsync(
                new GetVendorPaymentsByWeddingQuery(weddingId)))
            .Any(payment => payment.VendorAssignmentId == id))
        {
            TempData["ModuleMessage"] = "Elimina primero las cuotas y pagos vinculados a esta contratación.";
            return RedirectToAction(nameof(Index), new { weddingId });
        }

        var hasDocuments = (await getAllVendorDocuments.HandleAsync())
            .Any(document => document.VendorAssignmentId == id);
        if (hasDocuments)
        {
            TempData["ModuleMessage"] = "Elimina primero los documentos vinculados a esta asignación.";
            return RedirectToAction(nameof(Index), new { weddingId });
        }

        await deleteVendorAssignment.HandleAsync(new DeleteVendorAssignmentCommand(id));
        return RedirectToAction(nameof(Index), new { weddingId });
    }

    [HttpGet("Vendors/{assignmentId:guid}/Payments/Create")]
    public async Task<IActionResult> CreateVendorPayment(Guid weddingId, Guid assignmentId)
    {
        var assignment = await getVendorAssignmentById.HandleAsync(
            new GetVendorAssignmentByIdQuery(assignmentId));
        if (assignment is null || assignment.WeddingId != weddingId)
        {
            return NotFound();
        }

        return PartialView("Forms/_VendorPaymentForm", new VendorPaymentFormViewModel
        {
            WeddingId = weddingId,
            VendorAssignmentId = assignmentId
        });
    }

    [HttpGet("Vendors/{assignmentId:guid}/Payments/{id:guid}/Edit")]
    public async Task<IActionResult> EditVendorPayment(Guid weddingId, Guid assignmentId, Guid id)
    {
        var payment = await getVendorPaymentById.HandleAsync(new GetVendorPaymentByIdQuery(id));
        if (payment is null || payment.VendorAssignmentId != assignmentId
            || !await AssignmentBelongsToWedding(assignmentId, weddingId))
        {
            return NotFound();
        }

        return PartialView("Forms/_VendorPaymentForm", ToFormModel(payment, weddingId));
    }

    [HttpPost("Vendors/{assignmentId:guid}/Payments/Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateVendorPayment(
        Guid weddingId,
        Guid assignmentId,
        VendorPaymentFormViewModel model)
    {
        if (!await AssignmentBelongsToWedding(assignmentId, weddingId))
        {
            return NotFound();
        }

        model.Id = Guid.Empty;
        model.WeddingId = weddingId;
        model.VendorAssignmentId = assignmentId;
        if (!ModelState.IsValid)
        {
            return PartialView("Forms/_VendorPaymentForm", model);
        }

        try
        {
            await addVendorPayment.HandleAsync(
                new AddVendorPaymentCommand(ToEntity(model)));
        }
        catch (ValidationException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return PartialView("Forms/_VendorPaymentForm", model);
        }

        return RedirectToAction(nameof(Index), new { weddingId });
    }

    [HttpPost("Vendors/{assignmentId:guid}/Payments/{id:guid}/Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditVendorPayment(
        Guid weddingId,
        Guid assignmentId,
        Guid id,
        VendorPaymentFormViewModel model)
    {
        var payment = await getVendorPaymentById.HandleAsync(new GetVendorPaymentByIdQuery(id));
        if (payment is null || payment.VendorAssignmentId != assignmentId
            || !await AssignmentBelongsToWedding(assignmentId, weddingId))
        {
            return NotFound();
        }

        model.Id = id;
        model.WeddingId = weddingId;
        model.VendorAssignmentId = assignmentId;
        if (!ModelState.IsValid)
        {
            return PartialView("Forms/_VendorPaymentForm", model);
        }

        CopyToEntity(model, payment);
        try
        {
            await updateVendorPayment.HandleAsync(new UpdateVendorPaymentCommand(payment));
        }
        catch (ValidationException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return PartialView("Forms/_VendorPaymentForm", model);
        }

        return RedirectToAction(nameof(Index), new { weddingId });
    }

    [HttpPost("Vendors/{assignmentId:guid}/Payments/{id:guid}/Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteVendorPayment(Guid weddingId, Guid assignmentId, Guid id)
    {
        var payment = await getVendorPaymentById.HandleAsync(new GetVendorPaymentByIdQuery(id));
        if (payment is null || payment.VendorAssignmentId != assignmentId
            || !await AssignmentBelongsToWedding(assignmentId, weddingId))
        {
            return NotFound();
        }

        await deleteVendorPayment.HandleAsync(new DeleteVendorPaymentCommand(id));
        return RedirectToAction(nameof(Index), new { weddingId });
    }

    private async Task<bool> AssignmentBelongsToWedding(Guid assignmentId, Guid weddingId)
    {
        var assignment = await getVendorAssignmentById.HandleAsync(
            new GetVendorAssignmentByIdQuery(assignmentId));
        return assignment is not null && assignment.WeddingId == weddingId;
    }

    private async Task<bool> WeddingExists(Guid weddingId) =>
        await getWeddingById.HandleAsync(new GetWeddingByIdQuery(weddingId)) is not null;

    private async Task<bool> TaskBelongsToWedding(Guid taskId, Guid weddingId)
    {
        var task = await getTaskById.HandleAsync(new GetTaskItemByIdQuery(taskId));
        return task is not null && task.WeddingId == weddingId;
    }

    private async Task<IActionResult> VendorAssignmentForm(VendorAssignmentFormViewModel model)
    {
        ViewData["Vendors"] = await getAllVendors.HandleAsync();
        return PartialView("Forms/_VendorAssignmentForm", model);
    }

    private async Task<IActionResult> GuestForm(GuestFormViewModel model)
    {
        ViewData["WeddingTables"] = (await getWeddingSeating.HandleAsync(model.WeddingId)).Tables;
        return PartialView("Forms/_GuestForm", model);
    }

    private static GuestFormViewModel ToFormModel(Guest guest) => new()
    {
        Id = guest.Id,
        WeddingId = guest.WeddingId,
        FirstName = guest.FirstName,
        LastName = guest.LastName,
        Email = guest.Email,
        PhoneAreaCode = guest.Phone?.AreaCode,
        PhoneNumber = guest.Phone?.Number,
        RSVPStatus = guest.RSVPStatus,
        CanBringCompanion = guest.CanBringCompanion,
        CompanionStatus = guest.CompanionStatus,
        CompanionName = guest.CompanionName,
        CompanionMealPreference = guest.CompanionMealPreference,
        CompanionDietaryRestrictions = guest.CompanionDietaryRestrictions,
        CompanionNotes = guest.CompanionNotes,
        MealPreference = guest.MealPreference,
        WeddingTableId = guest.WeddingTableId,
        Notes = guest.Notes,
        IsFamily = guest.IsFamily,
        IsVIP = guest.IsVIP
    };

    private static Guest ToEntity(GuestFormViewModel model, Wedding wedding)
    {
        var guest = new Guest
        {
            WeddingId = model.WeddingId,
            Wedding = wedding,
            FirstName = model.FirstName,
            LastName = model.LastName,
            Email = model.Email,
            Phone = model.PhoneAreaCode is null && model.PhoneNumber is null
                ? null
                : new PhoneNumber { AreaCode = model.PhoneAreaCode, Number = model.PhoneNumber },
            RSVPStatus = model.RSVPStatus,
            MealPreference = model.MealPreference,
            WeddingTableId = model.WeddingTableId,
            Notes = model.Notes ?? string.Empty,
            IsFamily = model.IsFamily,
            IsVIP = model.IsVIP
        };
        CopyCompanionToEntity(model, guest);
        return guest;
    }

    private static void CopyToEntity(GuestFormViewModel model, Guest guest)
    {
        guest.FirstName = model.FirstName;
        guest.LastName = model.LastName;
        guest.Email = model.Email;
        if (model.PhoneAreaCode is null && model.PhoneNumber is null)
        {
            guest.Phone = null;
        }
        else
        {
            guest.Phone ??= new PhoneNumber();
            guest.Phone.AreaCode = model.PhoneAreaCode;
            guest.Phone.Number = model.PhoneNumber;
        }
        guest.RSVPStatus = model.RSVPStatus;
        guest.MealPreference = model.MealPreference;
        guest.WeddingTableId = model.WeddingTableId;
        guest.Notes = model.Notes ?? string.Empty;
        guest.IsFamily = model.IsFamily;
        guest.IsVIP = model.IsVIP;
        CopyCompanionToEntity(model, guest);
    }

    private static void CopyCompanionToEntity(GuestFormViewModel model, Guest guest)
    {
        guest.CanBringCompanion = model.CanBringCompanion;
        guest.CompanionStatus = model.CanBringCompanion
            ? model.CompanionStatus
            : CompanionAttendanceStatus.Pending;
        guest.CompanionName = model.CanBringCompanion ? model.CompanionName : null;
        guest.CompanionMealPreference = model.CanBringCompanion ? model.CompanionMealPreference : null;
        guest.CompanionDietaryRestrictions = model.CanBringCompanion ? model.CompanionDietaryRestrictions : null;
        guest.CompanionNotes = model.CanBringCompanion ? model.CompanionNotes : null;
    }

    private static BudgetFormViewModel ToFormModel(Budget budget) => new()
    {
        Id = budget.Id,
        WeddingId = budget.WeddingId,
        TotalAmount = budget.TotalAmount,
        CategoryAllocations = budget.CategoryAllocations
            .ToDictionary(allocation => allocation.Key, allocation => (decimal?)allocation.Value)
    };

    private static ExpenseFormViewModel ToFormModel(Expense expense) => new()
    {
        Id = expense.Id,
        BudgetId = expense.BudgetId,
        Category = expense.Category,
        Description = expense.Description ?? string.Empty,
        Amount = expense.Amount,
        VendorName = expense.VendorName,
        ExpenseDate = expense.ExpenseDate,
        DueDate = expense.DueDate,
        PaymentDate = expense.PaymentDate,
        IsRecurring = expense.IsRecurring,
        RecurrenceType = expense.RecurrenceType,
        Notes = expense.Notes,
        ReceiptUrl = expense.ReceiptUrl
    };

    private static Expense ToEntity(ExpenseFormViewModel model) => new()
    {
        BudgetId = model.BudgetId,
        Category = model.Category,
        Description = model.Description,
        Amount = model.Amount,
        VendorName = model.VendorName,
        ExpenseDate = model.ExpenseDate,
        DueDate = model.DueDate,
        PaymentDate = model.PaymentDate,
        IsRecurring = model.IsRecurring,
        RecurrenceType = model.IsRecurring ? model.RecurrenceType : null,
        Notes = model.Notes,
        ReceiptUrl = model.ReceiptUrl
    };

    private static void CopyToEntity(ExpenseFormViewModel model, Expense expense)
    {
        expense.Category = model.Category;
        expense.Description = model.Description;
        expense.Amount = model.Amount;
        expense.VendorName = model.VendorName;
        expense.ExpenseDate = model.ExpenseDate;
        expense.DueDate = model.DueDate;
        expense.PaymentDate = model.PaymentDate;
        expense.IsRecurring = model.IsRecurring;
        expense.RecurrenceType = model.IsRecurring ? model.RecurrenceType : null;
        expense.Notes = model.Notes;
        expense.ReceiptUrl = model.ReceiptUrl;
    }

    private static TaskItemFormViewModel ToFormModel(TaskItem task) => new()
    {
        Id = task.Id,
        WeddingId = task.WeddingId,
        Title = task.Title ?? string.Empty,
        Description = task.Description,
        DueDate = task.DueDate,
        Status = task.Status,
        Priority = task.Priority,
        Category = task.Category,
        EstimatedHours = task.EstimatedHours,
        ActualHours = task.ActualHours
    };

    private static TaskItem ToEntity(TaskItemFormViewModel model) => new()
    {
        WeddingId = model.WeddingId,
        Title = model.Title,
        Description = model.Description,
        DueDate = model.DueDate,
        Status = model.Status,
        Priority = model.Priority,
        Category = model.Category,
        EstimatedHours = model.EstimatedHours,
        ActualHours = model.ActualHours,
        CompletedAt = model.Status == TaskStatus.Completed ? DateTime.Now : null
    };

    private static void CopyToEntity(TaskItemFormViewModel model, TaskItem task)
    {
        task.Title = model.Title;
        task.Description = model.Description;
        task.DueDate = model.DueDate;
        task.Status = model.Status;
        task.Priority = model.Priority;
        task.Category = model.Category;
        task.EstimatedHours = model.EstimatedHours;
        task.ActualHours = model.ActualHours;
        task.CompletedAt = model.Status == TaskStatus.Completed
            ? task.CompletedAt ?? DateTime.Now
            : null;
    }

    private static ChecklistItemFormViewModel ToFormModel(ChecklistItem item, Guid weddingId) => new()
    {
        Id = item.Id,
        WeddingId = weddingId,
        TaskId = item.TaskId,
        Description = item.Description ?? string.Empty,
        IsCompleted = item.IsCompleted
    };

    private static TimelineFormViewModel ToFormModel(Timeline timeline) => new()
    {
        Id = timeline.Id,
        WeddingId = timeline.WeddingId,
        EventName = timeline.EventName ?? string.Empty,
        Description = timeline.Description,
        ScheduledDate = timeline.ScheduledDate,
        DurationMinutes = timeline.DurationMinutes,
        Location = timeline.Location,
        IsCompleted = timeline.IsCompleted,
        Category = timeline.Category,
        Notes = timeline.Notes
    };

    private static Timeline ToEntity(TimelineFormViewModel model) => new()
    {
        WeddingId = model.WeddingId,
        EventName = model.EventName,
        Description = model.Description,
        ScheduledDate = model.ScheduledDate,
        DurationMinutes = model.DurationMinutes,
        Location = model.Location,
        IsCompleted = model.IsCompleted,
        CompletedAt = model.IsCompleted ? DateTime.Now : null,
        Category = model.Category,
        Notes = model.Notes
    };

    private static void CopyToEntity(TimelineFormViewModel model, Timeline timeline)
    {
        timeline.EventName = model.EventName;
        timeline.Description = model.Description;
        timeline.ScheduledDate = model.ScheduledDate;
        timeline.DurationMinutes = model.DurationMinutes;
        timeline.Location = model.Location;
        timeline.IsCompleted = model.IsCompleted;
        timeline.CompletedAt = model.IsCompleted
            ? timeline.CompletedAt ?? DateTime.Now
            : null;
        timeline.Category = model.Category;
        timeline.Notes = model.Notes;
    }

    private static VendorAssignmentFormViewModel ToFormModel(VendorAssignment assignment) => new()
    {
        Id = assignment.Id,
        WeddingId = assignment.WeddingId,
        VendorId = assignment.VendorId,
        ContractDate = assignment.ContractDate,
        ContractEndDate = assignment.ContractEndDate,
        PaymentAmount = assignment.PaymentAmount,
        DepositPercentage = assignment.DepositPercentage,
        Notes = assignment.Notes
    };

    private static VendorPaymentFormViewModel ToFormModel(VendorPayment payment, Guid weddingId) => new()
    {
        Id = payment.Id,
        WeddingId = weddingId,
        VendorAssignmentId = payment.VendorAssignmentId,
        Amount = payment.Amount,
        Type = payment.Type,
        DueDate = payment.DueDate,
        PaymentDate = payment.PaymentDate,
        PaymentMethod = payment.PaymentMethod,
        ReceiptUrl = payment.ReceiptUrl,
        Notes = payment.Notes
    };

    private static VendorAssignment ToEntity(VendorAssignmentFormViewModel model, Vendor vendor) => new()
    {
        WeddingId = model.WeddingId,
        VendorId = vendor.Id,
        ServiceType = vendor.ServiceType,
        ContractDate = model.ContractDate,
        ContractEndDate = model.ContractEndDate,
        PaymentAmount = model.PaymentAmount,
        DepositPercentage = model.DepositPercentage,
        Notes = model.Notes
    };

    private static VendorPayment ToEntity(VendorPaymentFormViewModel model) => new()
    {
        VendorAssignmentId = model.VendorAssignmentId,
        Amount = model.Amount,
        Type = model.Type,
        DueDate = model.DueDate,
        PaymentDate = model.PaymentDate,
        PaymentMethod = model.PaymentMethod,
        ReceiptUrl = model.ReceiptUrl,
        Notes = model.Notes
    };

    private static void CopyToEntity(
        VendorAssignmentFormViewModel model,
        Vendor vendor,
        VendorAssignment assignment)
    {
        assignment.VendorId = vendor.Id;
        assignment.ServiceType = vendor.ServiceType;
        assignment.ContractDate = model.ContractDate;
        assignment.ContractEndDate = model.ContractEndDate;
        assignment.PaymentAmount = model.PaymentAmount;
        assignment.DepositPercentage = model.DepositPercentage;
        assignment.Notes = model.Notes;
    }

    private static void CopyToEntity(VendorPaymentFormViewModel model, VendorPayment payment)
    {
        payment.Amount = model.Amount;
        payment.Type = model.Type;
        payment.DueDate = model.DueDate;
        payment.PaymentDate = model.PaymentDate;
        payment.PaymentMethod = model.PaymentMethod;
        payment.ReceiptUrl = model.ReceiptUrl;
        payment.Notes = model.Notes;
    }
}
