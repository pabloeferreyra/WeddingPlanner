using System.ComponentModel.DataAnnotations;
using WeddingPlanner.Application.Abstractions.Persistence;
using WeddingPlanner.Application.Implementations;
using WeddingPlanner.Core.Domain.Entities;
using WeddingPlanner.Core.Domain.Enums;

namespace WeddingPlanner.Tests.Unit;

public class VendorPaymentTests
{
    [Fact]
    public void Payment_is_overdue_after_due_date_until_paid()
    {
        var payment = new VendorPayment
        {
            Amount = 50,
            DueDate = new DateTime(2026, 11, 25)
        };

        Assert.Equal(VendorPaymentStatus.Pending, payment.GetStatus(new DateOnly(2026, 11, 25)));
        Assert.Equal(VendorPaymentStatus.Overdue, payment.GetStatus(new DateOnly(2026, 11, 26)));

        payment.PaymentDate = new DateTime(2026, 11, 26);
        Assert.Equal(VendorPaymentStatus.Paid, payment.GetStatus(new DateOnly(2026, 11, 26)));
    }

    [Fact]
    public async Task Add_rejects_payment_that_exceeds_remaining_contract_balance()
    {
        var assignment = new VendorAssignment
        {
            PaymentAmount = 100,
            OpeningBalancePaid = 20
        };
        var paymentRepository = new StubPaymentRepository();
        var service = new VendorPaymentService(
            paymentRepository,
            new StubAssignmentRepository(assignment),
            new StubVendorPaymentRepository(70));

        await Assert.ThrowsAsync<ValidationException>(() => service.AddAsync(new VendorPayment
        {
            VendorAssignmentId = assignment.Id,
            Amount = 15
        }));

        Assert.Null(paymentRepository.Added);
    }

    [Fact]
    public async Task Update_excludes_current_payment_when_checking_contract_limit()
    {
        var assignment = new VendorAssignment
        {
            PaymentAmount = 100,
            OpeningBalancePaid = 10
        };
        var existingPayment = new VendorPayment
        {
            VendorAssignmentId = assignment.Id,
            Amount = 30
        };
        var paymentRepository = new StubPaymentRepository(existingPayment);
        var commitments = new StubVendorPaymentRepository(40);
        var service = new VendorPaymentService(
            paymentRepository,
            new StubAssignmentRepository(assignment),
            commitments);
        existingPayment.Amount = 50;

        var result = await service.UpdateAsync(existingPayment);

        Assert.Same(existingPayment, result);
        Assert.Same(existingPayment, paymentRepository.Updated);
        Assert.Equal(existingPayment.Id, commitments.ExcludedPaymentId);
    }

    [Fact]
    public async Task Updating_contract_rejects_amount_below_existing_commitments()
    {
        var existingAssignment = new VendorAssignment
        {
            PaymentAmount = 100,
            OpeningBalancePaid = 20
        };
        var repository = new StubAssignmentRepository(existingAssignment);
        var service = new VendorAssignmentService(
            repository,
            new StubVendorPaymentRepository(30));
        var updatedAssignment = new VendorAssignment
        {
            Id = existingAssignment.Id,
            PaymentAmount = 49,
            OpeningBalancePaid = existingAssignment.OpeningBalancePaid
        };

        await Assert.ThrowsAsync<ValidationException>(
            () => service.UpdateVendorAssignmentAsync(updatedAssignment));

        Assert.Null(repository.Updated);
    }

    [Fact]
    public async Task Adding_contract_rejects_opening_balance_above_contract_amount()
    {
        var repository = new StubAssignmentRepository();
        var service = new VendorAssignmentService(
            repository,
            new StubVendorPaymentRepository(0));

        await Assert.ThrowsAsync<ValidationException>(() =>
            service.AddVendorAssignmentAsync(new VendorAssignment
            {
                PaymentAmount = 100,
                OpeningBalancePaid = 101
            }));

        Assert.Null(repository.Added);
    }

    private sealed class StubPaymentRepository(VendorPayment? existing = null) : IRepository<VendorPayment>
    {
        public VendorPayment? Added { get; private set; }
        public VendorPayment? Updated { get; private set; }

        public Task<VendorPayment> AddAsync(VendorPayment entity)
        {
            Added = entity;
            return Task.FromResult(entity);
        }

        public Task<VendorPayment> GetByIdAsync(Guid id) =>
            Task.FromResult(existing!);

        public Task<IEnumerable<VendorPayment>> GetAllAsync() =>
            Task.FromResult<IEnumerable<VendorPayment>>([]);

        public Task<VendorPayment> UpdateAsync(VendorPayment entity)
        {
            Updated = entity;
            return Task.FromResult(entity);
        }

        public Task DeleteAsync(Guid id) => Task.CompletedTask;
        public Task<bool> ExistsAsync(Guid id) => Task.FromResult(false);
        public IQueryable<VendorPayment> AsQueryable() => Array.Empty<VendorPayment>().AsQueryable();
    }

    private sealed class StubAssignmentRepository(VendorAssignment? existing = null)
        : IRepository<VendorAssignment>
    {
        public VendorAssignment? Added { get; private set; }
        public VendorAssignment? Updated { get; private set; }

        public Task<VendorAssignment> AddAsync(VendorAssignment entity)
        {
            Added = entity;
            return Task.FromResult(entity);
        }

        public Task<VendorAssignment> GetByIdAsync(Guid id) => Task.FromResult(existing!);

        public Task<IEnumerable<VendorAssignment>> GetAllAsync() =>
            Task.FromResult<IEnumerable<VendorAssignment>>([]);

        public Task<VendorAssignment> UpdateAsync(VendorAssignment entity)
        {
            Updated = entity;
            return Task.FromResult(entity);
        }

        public Task DeleteAsync(Guid id) => Task.CompletedTask;
        public Task<bool> ExistsAsync(Guid id) => Task.FromResult(false);
        public IQueryable<VendorAssignment> AsQueryable() => Array.Empty<VendorAssignment>().AsQueryable();
    }

    private sealed class StubVendorPaymentRepository(decimal committedAmount)
        : IVendorPaymentRepository
    {
        public Guid? ExcludedPaymentId { get; private set; }

        public Task<IReadOnlyList<VendorPayment>> GetByWeddingAsync(Guid weddingId) =>
            Task.FromResult<IReadOnlyList<VendorPayment>>([]);

        public Task<decimal> GetTotalCommittedByAssignmentAsync(Guid assignmentId, Guid? excludedPaymentId = null)
        {
            ExcludedPaymentId = excludedPaymentId;
            return Task.FromResult(committedAmount);
        }
    }
}
