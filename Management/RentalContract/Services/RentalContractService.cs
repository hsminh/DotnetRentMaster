using Microsoft.EntityFrameworkCore;
using RentMaster.Core.Services;
using RentMaster.Data;
using RentMaster.Management.RealEstate.Repositories;
using RentMaster.Management.RentalContract.Repositories;
using RentMaster.Management.RentalContract.Types.Request;
using RentMaster.Management.RentalContract.Types.Response;

namespace RentMaster.Management.RentalContract.Services;

public class RentalContractService : BaseService<Models.RentalContract>
{
    private readonly RentalContractRepository _repository;
    private readonly ApartmentRepository _apartmentRepository;
    private readonly ApartmentRoomRepository _apartmentRoomRepository;
    private readonly AppDbContext _context;

    public RentalContractService(
        RentalContractRepository repository,
        ApartmentRepository apartmentRepository,
        ApartmentRoomRepository apartmentRoomRepository,
        AppDbContext context)
        : base(repository)
    {
        _repository = repository;
        _apartmentRepository = apartmentRepository;
        _apartmentRoomRepository = apartmentRoomRepository;
        _context = context;
    }

    public async Task<Models.RentalContract> CreateContractAsync(RentalContractCreateRequest request)
    {
        if (request.Type == "RoomBased")
        {
            var roomExists = await _context.ApartmentRooms.AnyAsync(r => r.Uid == request.ApartmentUid && !r.IsDelete);
            if (!roomExists)
                throw new InvalidOperationException($"Room with ID {request.ApartmentUid} does not exist.");
        }
        else if (request.Type == "FullApartment")
        {
            var apartmentExists = await _context.Apartments.AnyAsync(a => a.Uid == request.ApartmentUid && !a.IsDelete);
            if (!apartmentExists)
                throw new InvalidOperationException($"Apartment with ID {request.ApartmentUid} does not exist.");
        }
        else
        {
            throw new InvalidOperationException($"Invalid contract type: {request.Type}. Must be 'RoomBased' or 'FullApartment'.");
        }

        var contract = new Models.RentalContract()
        {
            LandlordUid = request.LandlordUid,
            ApartmentUid = request.ApartmentUid,
            Type = request.Type,
            ResponsibleUid = request.ResponsibleUid,
            ParticipantUids = request.ParticipantUids,
            MonthlyPrice = request.MonthlyPrice,
            DepositAmount = request.DepositAmount,
            StartDate = DateTime.SpecifyKind(request.StartDate, DateTimeKind.Utc),
            EndDate = request.EndDate.HasValue ? DateTime.SpecifyKind(request.EndDate.Value, DateTimeKind.Utc) : null
        };

        return await _repository.CreateAsync(contract);
    }

    public async Task<Models.RentalContract?> GetContractAsync(Guid uid)
    {
        return await _repository.GetAsync(c => c.Uid == uid);
    }
    public async Task<IEnumerable<RentalContractResponseDto>> GetContractsByParticipantAsync(Guid consumerUid)
    {
        var currentYear = DateTime.UtcNow.Year;
        var currentMonth = DateTime.UtcNow.Month;

        var contracts = await _context.RentalContracts
            .AsNoTracking()
            .Where(c =>
                !c.IsDelete &&
                c.ParticipantUids != null &&
                c.ParticipantUids.Any(p => p == consumerUid)
            )
            .ToListAsync();

        var contractUids = contracts.Select(c => c.Uid).ToList();

        var monthlyPayments = await _context.RentalContractMonthlyPayments
            .AsNoTracking()
            .Where(p =>
                contractUids.Contains(p.RentalContractUid) &&
                p.Year == currentYear &&
                p.Month == currentMonth &&
                !p.IsDelete
            )
            .ToListAsync();

        var result = contracts.Select(c =>
        {
            var payment = monthlyPayments.FirstOrDefault(p => p.RentalContractUid == c.Uid);
            var isPayment = payment == null || payment.IsPaid;

            return new RentalContractResponseDto
            {
                Uid = c.Uid,
                ConsumerUid = consumerUid,
                LandlordUid = c.LandlordUid,
                ApartmentUid = c.ApartmentUid,
                Type = c.Type,
                ResponsibleUid = c.ResponsibleUid,
                ParticipantUids = c.ParticipantUids,
                MonthlyPrice = c.MonthlyPrice,
                DepositAmount = c.DepositAmount,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                Status = c.Status.ToString(),
                CreatedAt = c.CreatedAt,
                ApartmentDetails = null,
                IsPayment = isPayment
            };
        }).ToList();

        return result;
    }

    public async Task<IEnumerable<Models.RentalContract>> GetContractsByLandlordAsync(Guid landlordUid)
    {
        return await _repository.FilterAsync(c => c.LandlordUid == landlordUid && !c.IsDelete);
    }

    public async Task<IEnumerable<Models.RentalContract>> GetContractsByApartmentAsync(Guid apartmentUid)
    {
        return await _repository.FilterAsync(c => c.ApartmentUid == apartmentUid && !c.IsDelete);
    }

    public async Task<Models.RentalContract?> UpdateContractAsync(Guid uid, RentalContractUpdateRequest request)
    {
        var contract = await _repository.GetAsync(c => c.Uid == uid);
        if (contract == null)
            return null;

        contract.Type = request.Type ?? contract.Type;
        contract.ResponsibleUid = request.ResponsibleUid ?? contract.ResponsibleUid;
        contract.ParticipantUids = request.ParticipantUids ?? contract.ParticipantUids;
        contract.MonthlyPrice = request.MonthlyPrice ?? contract.MonthlyPrice;
        contract.DepositAmount = request.DepositAmount ?? contract.DepositAmount;
        contract.StartDate = request.StartDate.HasValue ? DateTime.SpecifyKind(request.StartDate.Value, DateTimeKind.Utc) : contract.StartDate;
        contract.EndDate = request.EndDate.HasValue ? DateTime.SpecifyKind(request.EndDate.Value, DateTimeKind.Utc) : contract.EndDate;
        contract.Status = request.Status ?? contract.Status;

        await _repository.UpdateAsync(contract);
        return contract;
    }

    public async Task<bool> DeleteContractAsync(Guid uid)
    {
        var contract = await _repository.GetAsync(c => c.Uid == uid);
        if (contract == null)
            return false;

        await _repository.DeleteAsync(contract);
        return true;
    }
}
