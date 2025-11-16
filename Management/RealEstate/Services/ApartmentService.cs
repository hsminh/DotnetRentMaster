using RentMaster.Accounts.LandLords.Models;
using RentMaster.Core.File;
using RentMaster.Core.Services;
using RentMaster.Core.types.enums;
using RentMaster.RealEstate.Models;
using RentMaster.RealEstate.Types.Request;

namespace RentMaster.Management.RealEstate.Services;

public class ApartmentService : BaseService<Apartment>
{
    private readonly ApartmentRepository _apartmentRepository;
    private readonly FileService _fileService;

    public ApartmentService(ApartmentRepository apartmentRepository, FileService fileService)
        : base(apartmentRepository)
    {
        _apartmentRepository = apartmentRepository;
        _fileService = fileService;
    }
    
    public async Task<IEnumerable<Apartment>> getApartments(LandLord landlord)
    {
        return await _apartmentRepository.FilterAsync(a => a.LandlordUid == landlord.Uid);
    }
    
    public async Task<Apartment?> UpdateApartment(LandLord landlord, Guid uid, ApartmentCreateRequest request)
    {
        var apartment = await _apartmentRepository.GetAsync(a => a.LandlordUid == landlord.Uid && a.Uid == uid);
        if (apartment == null)
            return null;
        List<string>? imageUrls = null;
        if (request.Files != null && request.Files.Count > 0)
        {
            imageUrls = new List<string>();
            foreach (var file in request.Files)
            {
                var uploadResult = await _fileService.UploadFileAsync(
                    file,
                    landlord.Uid,
                    FileType.Image,
                    FileScope.Public
                );
                imageUrls.Add(uploadResult.Url);
            }
        }
        new Apartment(apartment, request, imageUrls);
        await _apartmentRepository.UpdateAsync(apartment);
        return apartment;
    }
    
    public async Task<Apartment?> GetApartment(LandLord landlord, Guid uid)
    {
        return await _apartmentRepository.GetAsync(a => a.LandlordUid == landlord.Uid && a.Uid == uid);
    }

    public async Task<Apartment> CreateApartmentAsync(LandLord landLord, ApartmentCreateRequest request)
    {
        var imageUrls = new List<string>();
        if (request.Files != null && request.Files.Count > 0)
        {
            foreach (var file in request.Files)
            {
                var uploadResult = await _fileService.UploadFileAsync(
                    file,
                    landLord.Uid,
                    FileType.Image,
                    FileScope.Public
                );

                imageUrls.Add(uploadResult.Url);
            }
        }
        var apartment = new Apartment(request, landLord.Uid, imageUrls);
        return await _apartmentRepository.CreateAsync(apartment);
    }
}