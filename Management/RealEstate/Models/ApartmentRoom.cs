using RentMaster.Core.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RentMaster.Addresses.Models;
using RentMaster.Management.RealEstate.Types.Request;

namespace RentMaster.RealEstate.Models;

[Table("apartment_rooms")]
public class ApartmentRoom : BaseModel
{
    [Required]
    public Guid ApartmentUid { get; set; }  

    [Required]
    public Guid LandlordUid { get; set; }

    [MaxLength(50)]
    public string RoomNumber { get; set; } = "#1";

    [Column(TypeName = "decimal(18,2)")]
    public decimal? Price { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? AreaLength { get; set; }

    public Guid? ProvinceDivisionUid { get; set; }

    public Guid? WardDivisionUid { get; set; }

    [MaxLength(4000)]
    [Column(TypeName = "varchar(4000)")]
    public string? MetaData { get; set; }

    
    [Column(TypeName = "decimal(18,2)")]
    public decimal? AreaWidth { get; set; }

    [MaxLength(50)]
    [EnumDataType(typeof(ApartmentStatus))]
    [Column(TypeName = "varchar(50)")]
    public string Status { get; set; } = string.Empty;

    public List<string> Images { get; set; } = new();
    public string Description { get; set; } = string.Empty;
    
    [ForeignKey(nameof(ProvinceDivisionUid))]
    public AddressDivision? Province { get; set; }

    [ForeignKey(nameof(WardDivisionUid))]
    public AddressDivision? Ward { get; set; }
    public ApartmentRoom() {}

    public ApartmentRoom(ApartmentRoomCreateRequest request, Guid landlordUid, Guid apartmentUid, List<string>? imageUrls = null)
    {
        LandlordUid = landlordUid;
        ApartmentUid = apartmentUid;
        RoomNumber = "#1";
        Price = request.Price;
        AreaLength = request.AreaLength;
        AreaWidth = request.AreaWidth;
        Status = request.Status;
        Description = request.Description;
        if (imageUrls != null && imageUrls.Count > 0)
            Images = imageUrls;
    }

    public void UpdateFromRequest(ApartmentRoomCreateRequest request, List<string>? imageUrls = null)
    {
        RoomNumber = "#1";
        Price = request.Price;
        AreaLength = request.AreaLength;
        AreaWidth = request.AreaWidth;
        Status = request.Status;
        Description = request.Description;
        if (imageUrls != null && imageUrls.Count > 0)
            Images = imageUrls;
    }
}
