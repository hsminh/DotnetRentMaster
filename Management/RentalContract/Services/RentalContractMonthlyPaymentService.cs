using Microsoft.EntityFrameworkCore;
using RentMaster.Core.Services;
using RentMaster.Data;
using RentMaster.Management.RentalContract.Repositories;
using RentMaster.Management.RentalContract.Types.Request;

namespace RentMaster.Management.RentalContract.Services;

public class RentalContractMonthlyPaymentService : BaseService<Models.RentalContractMonthlyPayment>
{
    private readonly RentalContractMonthlyPaymentRepository _repository;
    private readonly AppDbContext _context;

    public RentalContractMonthlyPaymentService(
        RentalContractMonthlyPaymentRepository repository,
        AppDbContext context)
        : base(repository)
    {
        _repository = repository;
        _context = context;
    }

    public async Task<Models.RentalContractMonthlyPayment> CreatePaymentAsync(
        RentalContractMonthlyPaymentCreateRequest request)
    {
        var contractExists = await _context.RentalContracts.AnyAsync(c =>
            c.Uid == request.RentalContractUid && !c.IsDelete);
        if (!contractExists)
            throw new InvalidOperationException(
                $"Rental contract with ID {request.RentalContractUid} does not exist.");

        var existingPayment = await _context.RentalContractMonthlyPayments.AnyAsync(p =>
            p.RentalContractUid == request.RentalContractUid &&
            p.Year == request.Year &&
            p.Month == request.Month &&
            !p.IsDelete);
        if (existingPayment)
            throw new InvalidOperationException(
                $"Payment for {request.Year}-{request.Month} already exists for this contract.");

        var payment = new Models.RentalContractMonthlyPayment
        {
            RentalContractUid = request.RentalContractUid,
            Year = request.Year,
            Month = request.Month,
            Amount = request.Amount,
            IsPaid = false,
            Note = request.Note,
            Method = request.Method
        };

        return await _repository.CreateAsync(payment);
    }
    
    public async Task<Models.RentalContractMonthlyPayment?> GetPaymentAsync(Guid uid)
    {
        return await _repository.GetAsync(p => p.Uid == uid);
    }

    public async Task<IEnumerable<Models.RentalContractMonthlyPayment>> GetPaymentsByContractAsync(
        Guid contractUid)
    {
        return await _repository.FilterAsync(p =>
            p.RentalContractUid == contractUid && !p.IsDelete);
    }


    public async Task<IEnumerable<Models.RentalContractMonthlyPayment>> GetUnpaidPaymentsAsync(
        Guid contractUid)
    {
        return await _repository.FilterAsync(p =>
            p.RentalContractUid == contractUid && !p.IsPaid && !p.IsDelete);
    }

    public async Task<Models.RentalContractMonthlyPayment?> MarkAsPaidAsync(
        Guid uid,
        Guid? collectedByUid = null,
        string? method = null,
        string? momoTransactionId = null,
        string? momoRequestId = null)
    {
        var payment = await _repository.GetAsync(p => p.Uid == uid);
        if (payment == null)
            return null;

        payment.IsPaid = true;
        payment.PaidAt = DateTime.UtcNow;
        payment.CollectedByUid = collectedByUid;
        payment.Method = method;
        if (!string.IsNullOrEmpty(momoTransactionId))
            payment.MoMoTransactionId = momoTransactionId;
        if (!string.IsNullOrEmpty(momoRequestId))
            payment.MoMoRequestId = momoRequestId;

        await _repository.UpdateAsync(payment);
        return payment;
    }

    public async Task<Models.RentalContractMonthlyPayment?> UpdatePaymentAsync(
        Guid uid,
        RentalContractMonthlyPaymentUpdateRequest request)
    {
        var payment = await _repository.GetAsync(p => p.Uid == uid);
        if (payment == null)
            return null;

        if (request.IsPaid.HasValue)
        {
            payment.IsPaid = request.IsPaid.Value;
            if (request.IsPaid.Value && !payment.PaidAt.HasValue)
            {
                payment.PaidAt = DateTime.UtcNow;
            }
            else if (!request.IsPaid.Value)
            {
                payment.PaidAt = null;
            }
        }

        payment.CollectedByUid = request.CollectedByUid ?? payment.CollectedByUid;
        payment.Note = request.Note ?? payment.Note;
        payment.Method = request.Method ?? payment.Method;

        await _repository.UpdateAsync(payment);
        return payment;
    }

    public async Task<bool> DeletePaymentAsync(Guid uid)
    {
        var payment = await _repository.GetAsync(p => p.Uid == uid);
        if (payment == null)
            return false;

        await _repository.DeleteAsync(payment);
        return true;
    }
}
