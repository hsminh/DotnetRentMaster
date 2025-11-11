using RentMaster.Accounts.Models;
using RentMaster.Core.File;
using RentMaster.Core.Services;
using RentMaster.Core.types.enums;
using RentMaster.RealEstate.Models;
using RentMaster.RealEstate.Repositories;
using RentMaster.RealEstate.Types.Request;

namespace RentMaster.RealEstate.Services;

public class ApartmentRoomService : BaseService<ApartmentRoom>
{
    private readonly ApartmentRoomRepository _apartmentRoomRepository;
    private readonly FileService _fileService;

    public ApartmentRoomService(ApartmentRoomRepository apartmentRoomRepository, FileService fileService)
        : base(apartmentRoomRepository)
    {
        _apartmentRoomRepository = apartmentRoomRepository;
        _fileService = fileService;
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

        List<string>? imageUrls = null;
        if (request.Files != null && request.Files.Count > 0)
        {
            imageUrls = new List<string>();
            foreach (var file in request.Files)
            {
                var uploadResult = await _fileService.UploadFileAsync(file, landlord.Uid, FileType.Image, FileScope.Public);
                imageUrls.Add(uploadResult.Url);
            }
        }

        room.UpdateFromRequest(request, imageUrls);
        await _apartmentRoomRepository.UpdateAsync(room);
        return room;
    }
}
