using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging.Abstractions;
using WeddingPlanner.Application.Abstractions.Persistence;
using WeddingPlanner.Application.Implementations;
using WeddingPlanner.Core.Domain.Entities;
using WeddingPlanner.Core.Domain.Enums;
using WeddingPlanner.Web.Models;

namespace WeddingPlanner.Tests.Unit;

public class GuestCompanionTests
{
    [Fact]
    public void Companion_confirmation_requires_permission()
    {
        var model = CreateGuestFormModel(canBringCompanion: false);
        model.CompanionStatus = CompanionAttendanceStatus.Confirmed;
        var validationResults = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(
            model,
            new ValidationContext(model),
            validationResults,
            validateAllProperties: true);

        Assert.False(isValid);
        Assert.Contains(validationResults, result =>
            result.MemberNames.Contains(nameof(GuestFormViewModel.CanBringCompanion)));
    }

    [Fact]
    public void Confirmed_attendees_count_main_guests_and_permitted_companions_separately()
    {
        var viewModel = new WeddingPlanningViewModel
        {
            Wedding = new Wedding
            {
                Title = "Ana y Luis",
                WeddingDate = new DateOnly(2027, 5, 1)
            },
            Guests =
            [
                CreateGuest(GuestStatus.Confirmed, canBringCompanion: true, CompanionAttendanceStatus.Confirmed),
                CreateGuest(GuestStatus.Pending, canBringCompanion: true, CompanionAttendanceStatus.Pending),
                CreateGuest(GuestStatus.Confirmed, canBringCompanion: false, CompanionAttendanceStatus.Confirmed),
                CreateGuest(GuestStatus.Declined, canBringCompanion: true, CompanionAttendanceStatus.Confirmed)
            ]
        };

        Assert.Equal(2, viewModel.ConfirmedMainGuestCount);
        Assert.Equal(1, viewModel.ConfirmedCompanionCount);
        Assert.Equal(3, viewModel.ConfirmedAttendeeCount);
    }

    [Fact]
    public async Task Declining_main_guest_marks_permitted_companion_as_declined()
    {
        var repository = new StubGuestRepository();
        var service = CreateGuestService(repository);
        var guest = CreateGuest(
            GuestStatus.Declined,
            canBringCompanion: true,
            CompanionAttendanceStatus.Confirmed);

        await service.UpdateGuestAsync(guest);

        Assert.Equal(CompanionAttendanceStatus.Declined, guest.CompanionStatus);
        Assert.Same(guest, repository.UpdatedGuest);
    }

    [Fact]
    public async Task Guest_without_companion_permission_cannot_persist_companion_details()
    {
        var repository = new StubGuestRepository();
        var service = CreateGuestService(repository);
        var guest = CreateGuest(
            GuestStatus.Confirmed,
            canBringCompanion: false,
            CompanionAttendanceStatus.Confirmed);
        guest.CompanionName = "Luis";
        guest.CompanionMealPreference = MealPreference.Vegetarian;
        guest.CompanionDietaryRestrictions = "Sin gluten";
        guest.CompanionNotes = "Observación";

        await service.AddGuestAsync(guest);

        Assert.Equal(CompanionAttendanceStatus.Pending, guest.CompanionStatus);
        Assert.Null(guest.CompanionName);
        Assert.Null(guest.CompanionMealPreference);
        Assert.Null(guest.CompanionDietaryRestrictions);
        Assert.Null(guest.CompanionNotes);
        Assert.Same(guest, repository.AddedGuest);
    }

    private static GuestService CreateGuestService(StubGuestRepository repository) =>
        new(repository, repository, NullLogger<GuestService>.Instance);

    private static GuestFormViewModel CreateGuestFormModel(bool canBringCompanion) => new()
    {
        WeddingId = Guid.NewGuid(),
        FirstName = "Ana",
        LastName = "Pérez",
        Email = "ana@example.com",
        CanBringCompanion = canBringCompanion
    };

    private static Guest CreateGuest(
        GuestStatus rsvpStatus,
        bool canBringCompanion,
        CompanionAttendanceStatus companionStatus) => new()
    {
        WeddingId = Guid.NewGuid(),
        Wedding = new Wedding { Title = "Celebración" },
        FirstName = "Ana",
        LastName = "Pérez",
        Email = "ana@example.com",
        Notes = string.Empty,
        RSVPStatus = rsvpStatus,
        CanBringCompanion = canBringCompanion,
        CompanionStatus = companionStatus
    };

    private sealed class StubGuestRepository : IRepository<Guest>, IGuestRepository
    {
        public Guest? AddedGuest { get; private set; }
        public Guest? UpdatedGuest { get; private set; }

        public Task<Guest> AddAsync(Guest entity)
        {
            AddedGuest = entity;
            return Task.FromResult(entity);
        }

        public Task<Guest> UpdateAsync(Guest entity)
        {
            UpdatedGuest = entity;
            return Task.FromResult(entity);
        }

        public Task<Guest> GetByIdAsync(Guid id) => throw new NotSupportedException();
        public Task<IEnumerable<Guest>> GetAllAsync() => throw new NotSupportedException();
        public Task DeleteAsync(Guid id) => throw new NotSupportedException();
        public Task<bool> ExistsAsync(Guid id) => throw new NotSupportedException();
        public IQueryable<Guest> AsQueryable() => throw new NotSupportedException();
        public Task<IReadOnlyList<Guest>> GetGuestsByWedding(Guid weddingId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<IReadOnlyList<Guest>> GetGuestsByStatus(Guid weddingId, GuestStatus status, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }
}
