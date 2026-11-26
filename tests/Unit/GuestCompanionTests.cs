using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging.Abstractions;
using WeddingPlanner.Application.Abstractions.Persistence;
using WeddingPlanner.Application.Implementations;
using WeddingPlanner.Core.Domain.Entities;
using WeddingPlanner.Core.Domain.Enums;
using WeddingPlanner.Core.Domain.Services;
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

    [Theory]
    [InlineData(GuestStatus.Confirmed, CompanionAttendanceStatus.Confirmed, true, 1, 1, 2)]
    [InlineData(GuestStatus.Confirmed, CompanionAttendanceStatus.Pending, true, 1, 0, 1)]
    [InlineData(GuestStatus.Confirmed, CompanionAttendanceStatus.Declined, true, 1, 0, 1)]
    [InlineData(GuestStatus.Pending, CompanionAttendanceStatus.Confirmed, true, 0, 0, 0)]
    [InlineData(GuestStatus.Pending, CompanionAttendanceStatus.Pending, true, 0, 0, 0)]
    [InlineData(GuestStatus.Pending, CompanionAttendanceStatus.Declined, true, 0, 0, 0)]
    [InlineData(GuestStatus.Declined, CompanionAttendanceStatus.Confirmed, true, 0, 0, 0)]
    [InlineData(GuestStatus.Declined, CompanionAttendanceStatus.Pending, true, 0, 0, 0)]
    [InlineData(GuestStatus.Declined, CompanionAttendanceStatus.Declined, true, 0, 0, 0)]
    [InlineData(GuestStatus.Confirmed, CompanionAttendanceStatus.Confirmed, false, 1, 0, 1)]
    public void Attendance_requires_a_confirmed_guest_and_permitted_confirmed_companion(
        GuestStatus guestStatus,
        CompanionAttendanceStatus companionStatus,
        bool canBringCompanion,
        int mainGuests,
        int companions,
        int attendees)
    {
        var result = GuestAttendanceCalculator.Calculate(
            [CreateGuest(guestStatus, canBringCompanion, companionStatus)]);

        Assert.Equal(mainGuests, result.ConfirmedMainGuestCount);
        Assert.Equal(companions, result.ConfirmedCompanionCount);
        Assert.Equal(attendees, result.ConfirmedAttendeeCount);
    }

    [Fact]
    public void Attendance_is_zero_for_an_empty_collection()
    {
        var result = GuestAttendanceCalculator.Calculate([]);

        Assert.Equal(0, result.ConfirmedMainGuestCount);
        Assert.Equal(0, result.ConfirmedCompanionCount);
        Assert.Equal(0, result.ConfirmedAttendeeCount);
    }

    [Fact]
    public void Attendance_sums_valid_confirmations_across_guests()
    {
        var result = GuestAttendanceCalculator.Calculate(
        [
            CreateGuest(GuestStatus.Confirmed, true, CompanionAttendanceStatus.Confirmed),
            CreateGuest(GuestStatus.Confirmed, true, CompanionAttendanceStatus.Pending),
            CreateGuest(GuestStatus.Confirmed, false, CompanionAttendanceStatus.Pending),
            CreateGuest(GuestStatus.Pending, true, CompanionAttendanceStatus.Confirmed),
            CreateGuest(GuestStatus.Declined, true, CompanionAttendanceStatus.Confirmed),
            CreateGuest(GuestStatus.Confirmed, true, CompanionAttendanceStatus.Confirmed)
        ]);

        Assert.Equal(4, result.ConfirmedMainGuestCount);
        Assert.Equal(2, result.ConfirmedCompanionCount);
        Assert.Equal(6, result.ConfirmedAttendeeCount);
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
        new(repository, repository, NullLogger<GuestService>.Instance, new StubWeddingTableRepository());

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

    private sealed class StubWeddingTableRepository : IWeddingTableRepository
    {
        public Task<T> WithWeddingLockAsync<T>(Guid weddingId, Func<Task<T>> action) => action();
        public Task<IReadOnlyList<WeddingTable>> GetByWeddingAsync(Guid weddingId) => throw new NotSupportedException();
        public Task<WeddingTable?> GetByIdAsync(Guid id) => throw new NotSupportedException();
        public Task<IReadOnlyList<Guest>> GetGuestsByWeddingAsync(Guid weddingId) => throw new NotSupportedException();
        public Task<Guest?> GetStoredGuestAsync(Guid id) => throw new NotSupportedException();
        public Task<WeddingTable> AddAsync(WeddingTable table) => throw new NotSupportedException();
        public Task<WeddingTable> UpdateAsync(WeddingTable table) => throw new NotSupportedException();
        public Task DeleteAsync(WeddingTable table) => throw new NotSupportedException();
    }
}
