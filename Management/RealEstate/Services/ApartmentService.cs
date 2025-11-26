using Microsoft.EntityFrameworkCore;
using RentMaster.Accounts.LandLords.Models;
using RentMaster.Addresses.Models;
using RentMaster.Core.File;
using RentMaster.Core.Services;
using RentMaster.Core.types.enums;
using RentMaster.Data;
using RentMaster.Management.RealEstate.Models;
using RentMaster.Management.RealEstate.Types.Request;
using RentMaster.Management.RealEstate.Types.Response;

namespace RentMaster.Management.RealEstate.Services;

public class ApartmentService : BaseService<Apartment>
{
    private readonly ApartmentRepository _apartmentRepository;
    private readonly FileService _fileService;
    private readonly AppDbContext _context;

    public ApartmentService(ApartmentRepository apartmentRepository, FileService fileService, AppDbContext context)
        : base(apartmentRepository)
    {
        _apartmentRepository = apartmentRepository;
        _fileService = fileService;
        _context = context;
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
public async Task<IEnumerable<ApartmentResponse>> GetFullApartments(ApartmentFilterRequest? filter = null)
{
    var apartments = await _apartmentRepository.FilterAsync(a => 
        !a.IsDelete && a.Type == ApartmentType.FullApartment.ToString());

    if (filter != null)
    {
        if (filter.MinPrice.HasValue)
            apartments = apartments.Where(a => a.Price >= filter.MinPrice.Value).ToList();

        if (filter.MaxPrice.HasValue)
            apartments = apartments.Where(a => a.Price <= filter.MaxPrice.Value).ToList();

        if (filter.WardDivisionUid.HasValue)
            apartments = apartments.Where(a => a.WardDivisionUid == filter.WardDivisionUid.Value).ToList();

        if (filter.ProvinceDivisionUid.HasValue)
            apartments = apartments.Where(a => a.ProvinceDivisionUid == filter.ProvinceDivisionUid.Value).ToList();
    }

    var provinceIds = apartments.Select(a => a.ProvinceDivisionUid).Where(id => id.HasValue).Cast<Guid>().ToList();
    var wardIds = apartments.Select(a => a.WardDivisionUid).Where(id => id.HasValue).Cast<Guid>().ToList();
    
    // Load all related entities in one query each
    var provinces = await _context.AddressDivisions
        .Where(a => provinceIds.Contains(a.Uid))
        .ToDictionaryAsync(a => a.Uid);
        
    var wards = await _context.AddressDivisions
        .Where(a => wardIds.Contains(a.Uid))
        .ToDictionaryAsync(a => a.Uid);
    
    // Assign related entities
    foreach (var apartment in apartments)
    {
        if (apartment.ProvinceDivisionUid.HasValue && 
            provinces.TryGetValue(apartment.ProvinceDivisionUid.Value, out var province))
        {
            apartment.Province = province;
        }
        
        if (apartment.WardDivisionUid.HasValue && 
            wards.TryGetValue(apartment.WardDivisionUid.Value, out var ward))
        {
            apartment.Ward = ward;
        }
    }

    // Apply province name filter after loading related data
    if (filter != null && !string.IsNullOrEmpty(filter.ProvinceName))
    {
        apartments = apartments.Where(a => 
            a.Province != null && 
            a.Province.Name.Contains(filter.ProvinceName, StringComparison.OrdinalIgnoreCase)
        ).ToList();
    }
        
    return apartments.Select(ApartmentResponse.FromApartment);
}
}