using System.ComponentModel.DataAnnotations;
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
                CreateGuest(GuestStatus.Confirmed, canBringCompanion: false, CompanionAttendanceStatus.Confirmed)
            ]
        };

        Assert.Equal(2, viewModel.ConfirmedMainGuestCount);
        Assert.Equal(1, viewModel.ConfirmedCompanionCount);
        Assert.Equal(3, viewModel.ConfirmedAttendeeCount);
    }

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
}
