using Microsoft.AspNetCore.Mvc;
using WeddingPlanner.Application.UseCases.Budgets;
using WeddingPlanner.Application.UseCases.ChecklistItems;
using WeddingPlanner.Application.UseCases.Expenses;
using WeddingPlanner.Application.UseCases.Guests;
using WeddingPlanner.Application.UseCases.TaskItems;
using WeddingPlanner.Application.UseCases.Timelines;
using WeddingPlanner.Application.UseCases.VendorAssignments;
using WeddingPlanner.Application.UseCases.VendorDocuments;
using WeddingPlanner.Application.UseCases.Vendors;
using WeddingPlanner.Application.UseCases.Weddings;
using WeddingPlanner.Core.Domain.Entities;
using WeddingPlanner.Core.Domain.Enums;
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
    GetAllVendorDocumentsHandler getAllVendorDocuments,
    GetAllVendorsHandler getAllVendors)
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
        var guests = await getGuestsByWedding.HandleAsync(
            new GetGuestsByWeddingQuery(weddingId),
            cancellationToken);
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
        ViewData["ConfirmedGuestCount"] = guests.Count(guest =>
            guest.RSVPStatus == GuestStatus.Confirmed);
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
            Vendors = (await getAllVendors.HandleAsync()).ToList()
        });
    }

    [HttpGet("Guests/Create")]
    public async Task<IActionResult> CreateGuest(Guid weddingId)
    {
        if (!await WeddingExists(weddingId))
        {
            return NotFound();
        }

        return PartialView("Forms/_GuestForm", new GuestFormViewModel { WeddingId = weddingId });
    }

    [HttpGet("Guests/{id:guid}/Edit")]
    public async Task<IActionResult> EditGuest(Guid weddingId, Guid id)
    {
        var guest = await getGuestById.HandleAsync(new GetGuestByIdQuery(id));
        if (guest is null || guest.WeddingId != weddingId)
        {
            return NotFound();
        }

        return PartialView("Forms/_GuestForm", ToFormModel(guest));
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
            return PartialView("Forms/_GuestForm", model);
        }

        await addGuest.HandleAsync(new AddGuestCommand(ToEntity(model, wedding)));
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
            return PartialView("Forms/_GuestForm", model);
        }

        CopyToEntity(model, guest);
        await updateGuest.HandleAsync(new UpdateGuestCommand(guest));
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

        await addBudget.HandleAsync(new AddBudgetCommand(new Budget
        {
            WeddingId = weddingId,
            TotalAmount = model.TotalAmount,
            CurrentSpent = model.CurrentSpent
        }));
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
        budget.CurrentSpent = model.CurrentSpent;
        await updateBudget.HandleAsync(new UpdateBudgetCommand(budget));
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
        ValidateExpenseForm(model);
        if (!ModelState.IsValid)
        {
            return PartialView("Forms/_ExpenseForm", model);
        }

        await addExpense.HandleAsync(new AddExpenseCommand(ToEntity(model)));
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
        ValidateExpenseForm(model);
        if (!ModelState.IsValid)
        {
            return PartialView("Forms/_ExpenseForm", model);
        }

        CopyToEntity(model, expense);
        await updateExpense.HandleAsync(new UpdateExpenseCommand(expense));
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

        await addVendorAssignment.HandleAsync(new AddVendorAssignmentCommand(ToEntity(model, vendor!)));
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
        await updateVendorAssignment.HandleAsync(new UpdateVendorAssignmentCommand(assignment));
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

    private void ValidateExpenseForm(ExpenseFormViewModel model)
    {
        if (model.IsRecurring && model.RecurrenceType is null)
        {
            ModelState.AddModelError(nameof(model.RecurrenceType), "Selecciona la frecuencia del gasto recurrente.");
        }
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
        MealPreference = guest.MealPreference,
        TableNumber = guest.TableNumber,
        Notes = guest.Notes,
        IsFamily = guest.IsFamily,
        IsVIP = guest.IsVIP
    };

    private static Guest ToEntity(GuestFormViewModel model, Wedding wedding) => new()
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
        TableNumber = model.TableNumber,
        Notes = model.Notes ?? string.Empty,
        IsFamily = model.IsFamily,
        IsVIP = model.IsVIP
    };

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
        guest.TableNumber = model.TableNumber;
        guest.Notes = model.Notes ?? string.Empty;
        guest.IsFamily = model.IsFamily;
        guest.IsVIP = model.IsVIP;
    }

    private static BudgetFormViewModel ToFormModel(Budget budget) => new()
    {
        Id = budget.Id,
        WeddingId = budget.WeddingId,
        TotalAmount = budget.TotalAmount,
        CurrentSpent = budget.CurrentSpent
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
        DepositPaid = assignment.DepositPaid,
        TotalPaid = assignment.TotalPaid,
        PaymentStatus = assignment.PaymentStatus,
        Notes = assignment.Notes
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
        DepositPaid = model.DepositPaid,
        TotalPaid = model.TotalPaid,
        PaymentStatus = model.PaymentStatus,
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
        assignment.DepositPaid = model.DepositPaid;
        assignment.TotalPaid = model.TotalPaid;
        assignment.PaymentStatus = model.PaymentStatus;
        assignment.Notes = model.Notes;
    }
}
