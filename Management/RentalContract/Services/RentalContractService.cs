using Microsoft.EntityFrameworkCore;
using RentMaster.Core.Services;
using RentMaster.Data;
using RentMaster.Management.RealEstate.Models;
using RentMaster.Management.RealEstate.Repositories;
using RentMaster.Management.RentalContract.Repositories;
using RentMaster.Management.RentalContract.Types.Request;

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
            ConsumerUid = request.ConsumerUid,
            LandlordUid = request.LandlordUid,
            ApartmentUid = request.ApartmentUid,
            Type = request.Type,
            ResponsibleUid = request.ResponsibleUid,
            ParticipantUids = request.ParticipantUids,
            MonthlyPrice = request.MonthlyPrice,
            DepositAmount = request.DepositAmount,
            StartDate = request.StartDate,
            EndDate = request.EndDate
        };

        return await _repository.CreateAsync(contract);
    }

    public async Task<Models.RentalContract?> GetContractAsync(Guid uid)
    {
        return await _repository.GetAsync(c => c.Uid == uid);
    }

    public async Task<IEnumerable<Models.RentalContract>> GetContractsByLandlordAsync(Guid landlordUid)
    {
        return await _repository.FilterAsync(c => c.LandlordUid == landlordUid && !c.IsDelete);
    }

    public async Task<IEnumerable<Models.RentalContract>> GetContractsByConsumerAsync(Guid consumerUid)
    {
        return await _repository.FilterAsync(c => c.ConsumerUid == consumerUid && !c.IsDelete);
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
        contract.StartDate = request.StartDate ?? contract.StartDate;
        contract.EndDate = request.EndDate ?? contract.EndDate;
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
