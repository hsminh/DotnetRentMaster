using Microsoft.EntityFrameworkCore;
using RentMaster.Accounts.LandLords.Models;
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
        var query = _context.Apartments
            .Where(a => !a.IsDelete && a.Type == ApartmentType.FullApartment.ToString())
            .AsQueryable();

        // Apply filters
        if (filter != null)
        {
            if (filter.MinPrice.HasValue)
                query = query.Where(a => a.Price >= filter.MinPrice.Value);

            if (filter.MaxPrice.HasValue)
                query = query.Where(a => a.Price <= filter.MaxPrice.Value);

            if (filter.WardDivisionUid.HasValue)
                query = query.Where(a => a.WardDivisionUid == filter.WardDivisionUid.Value);

            if (filter.ProvinceDivisionUid.HasValue)
                query = query.Where(a => a.ProvinceDivisionUid == filter.ProvinceDivisionUid.Value);
                
            if (filter.StreetUid.HasValue)
                query = query.Where(a => a.StreetUid == filter.StreetUid.Value);
        }

        // Get all apartments with related data
        var apartments = await query
            .Include(a => a.Province)
            .Include(a => a.Ward)
            .Include(a => a.Street)
            .ToListAsync();

        // Apply any remaining filters that need in-memory processing
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