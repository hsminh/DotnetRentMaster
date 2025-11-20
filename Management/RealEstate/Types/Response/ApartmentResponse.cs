using RentMaster.Addresses.Models;
using RentMaster.Management.RealEstate.Models;

namespace RentMaster.Management.RealEstate.Types.Response;

public class ApartmentResponse
{
    public Guid Uid { get; set; }
    public Guid LandlordUid { get; set; }
    public decimal? Price { get; set; }
    public string Pid { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal? AreaLength { get; set; }
    public decimal? AreaWidth { get; set; }
    public string? MetaData { get; set; }
    public int? TotalFloors { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public List<string> Images { get; set; } = new();
    public AddressDivision? Province { get; set; }
    public AddressDivision? Ward { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDelete { get; set; }

    public static ApartmentResponse FromApartment(Apartment apartment)
    {
        return new ApartmentResponse
        {
            Uid = apartment.Uid,
            LandlordUid = apartment.LandlordUid,
            Price = apartment.Price,
            Pid = apartment.Pid,
            Title = apartment.Title,
            Description = apartment.Description,
            AreaLength = apartment.AreaLength,
            AreaWidth = apartment.AreaWidth,
            MetaData = apartment.MetaData,
            TotalFloors = apartment.TotalFloors,
            Type = apartment.Type,
            Status = apartment.Status,
            Images = apartment.Images,
            Province = apartment.Province,
            Ward = apartment.Ward,
            CreatedAt = apartment.CreatedAt,
            UpdatedAt = apartment.UpdatedAt,
            IsDelete = apartment.IsDelete
        };
    }
}