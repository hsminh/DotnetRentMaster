using Microsoft.EntityFrameworkCore;
using RentMaster.Accounts.LandLords.Models;
using RentMaster.Core.File;
using RentMaster.Core.Services;
using RentMaster.Core.types.enums;
using RentMaster.Data;
using RentMaster.Management.RealEstate.Models;
using RentMaster.Management.RealEstate.Repositories;
using RentMaster.Management.RealEstate.Types.Request;

namespace RentMaster.Management.RealEstate.Services
{
    public class ApartmentRoomService : BaseService<ApartmentRoom>
    {
        private readonly ApartmentRoomRepository _apartmentRoomRepository;
        private readonly FileService _fileService;
        private readonly AppDbContext _context;

        public ApartmentRoomService(ApartmentRoomRepository apartmentRoomRepository, FileService fileService, AppDbContext context)
            : base(apartmentRoomRepository)
        {
            _apartmentRoomRepository = apartmentRoomRepository;
            _fileService = fileService;
            _context = context;
        }

        public async Task<IEnumerable<ApartmentRoom>> GetApartmentRooms(LandLord landlord)
        {
            return await _apartmentRoomRepository.FilterAsync(a => a.LandlordUid == landlord.Uid);
        }

        public async Task<ApartmentRoom?> GetApartmentRoom(LandLord landlord, Guid uid)
        {
            return await _apartmentRoomRepository.GetAsync(a => a.LandlordUid == landlord.Uid && a.Uid == uid);
        }

        public async Task<ApartmentRoom> CreateApartmentRoomAsync(LandLord landlord, ApartmentRoomCreateRequest request)
        {
            var imageUrls = new List<string>();
            if (request.Files != null && request.Files.Count > 0)
            {
                foreach (var file in request.Files)
                {
                    var uploadResult = await _fileService.UploadFileAsync(file, landlord.Uid, FileType.Image, FileScope.Public);
                    imageUrls.Add(uploadResult.Url);
                }
            }

            var room = new ApartmentRoom(request, landlord.Uid, request.ApartmentUid, imageUrls);
            return await _apartmentRoomRepository.CreateAsync(room);
        }

        public async Task<ApartmentRoom?> UpdateApartmentRoomAsync(LandLord landlord, Guid uid, ApartmentRoomCreateRequest request)
        {
            var room = await _apartmentRoomRepository.GetAsync(a => a.LandlordUid == landlord.Uid && a.Uid == uid);
            if (room == null)
                return null;

            var imageUrls = new List<string>();
            if (request.Files != null && request.Files.Count > 0)
            {
                foreach (var file in request.Files)
                {
                    var uploadResult = await _fileService.UploadFileAsync(file, landlord.Uid, FileType.Image, FileScope.Public);
                    imageUrls.Add(uploadResult.Url);
                }
            }

            room.UpdateFromRequest(request, imageUrls.Count > 0 ? imageUrls : null);
            await _apartmentRoomRepository.UpdateAsync(room);
            return room;
        }
        
    public async Task<IEnumerable<ApartmentRoom>> GetAllRooms(RoomFilterRequest? filter = null)
{
    var apartmentQuery = _context.Apartments.Where(a => !a.IsDelete);

    if (filter != null)
    {
        if (filter.ProvinceDivisionUid.HasValue)
            apartmentQuery = apartmentQuery.Where(a => a.ProvinceDivisionUid == filter.ProvinceDivisionUid.Value);

        if (filter.WardDivisionUid.HasValue)
            apartmentQuery = apartmentQuery.Where(a => a.WardDivisionUid == filter.WardDivisionUid.Value);

        if (!string.IsNullOrEmpty(filter.ProvinceName))
        {
            apartmentQuery = from apartment in apartmentQuery
                            join province in _context.AddressDivisions 
                                on apartment.ProvinceDivisionUid equals province.Uid
                            where province.Name.Contains(filter.ProvinceName)
                            select apartment;
        }

        if (!string.IsNullOrEmpty(filter.AddressDetail))
        {
            apartmentQuery = apartmentQuery.Where(a => 
                a.MetaData != null && 
                a.MetaData.Contains(filter.AddressDetail)
            );
        }
    }

    var filteredApartments = await apartmentQuery.ToListAsync();
    var apartmentUids = filteredApartments.Select(a => a.Uid).ToList();

    if (!apartmentUids.Any())
        return new List<ApartmentRoom>();

    var roomQuery = _context.ApartmentRooms
        .Where(r => !r.IsDelete && apartmentUids.Contains(r.ApartmentUid));

    if (filter != null)
    {
        if (filter.MinPrice.HasValue)
            roomQuery = roomQuery.Where(r => r.Price >= filter.MinPrice.Value);

        if (filter.MaxPrice.HasValue)
            roomQuery = roomQuery.Where(r => r.Price <= filter.MaxPrice.Value);

        if (filter.ApartmentUid.HasValue)
            roomQuery = roomQuery.Where(r => r.ApartmentUid == filter.ApartmentUid.Value);
    }

    var rooms = await roomQuery.ToListAsync();

    var roomWithApartmentInfo = rooms.Select(room => 
    {
        return room;
    }).ToList();

    return roomWithApartmentInfo;
}

    }
    
}
