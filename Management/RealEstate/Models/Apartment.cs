using RentMaster.Core.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using RentMaster.Core.Utils;
using RentMaster.Addresses.Models;
using RentMaster.Management.RealEstate.Types.Request;

namespace RentMaster.Management.RealEstate.Models;
[Table("apartments")]
public class Apartment : BaseModel
{
    [Required]
    public Guid LandlordUid { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? Price { get; set; }

    [MaxLength(15)] 
    public string Pid { get; set; } = PidGenerator.GeneratePid(10);

    [MaxLength(255)]
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public Guid? AddressDivisionUid { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? AreaLength { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? AreaWidth { get; set; }

    public Guid? ProvinceDivisionUid { get; set; }

    public Guid? WardDivisionUid { get; set; }

    [MaxLength(4000)]
    [Column(TypeName = "varchar(4000)")]
    public string? MetaData { get; set; }


    public int? TotalFloors { get; set; }

    [Required, Column(TypeName = "varchar(50)")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public string Type { get; set; } = string.Empty;

    [Required, Column(TypeName = "varchar(50)")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public string Status { get; set; } = string.Empty; 
    
    public List<string> Images { get; set; } = new();
    
    [ForeignKey(nameof(ProvinceDivisionUid))]
    public AddressDivision? Province { get; set; }

    [ForeignKey(nameof(WardDivisionUid))]
    public AddressDivision? Ward { get; set; }
    public Apartment() {}

    public Apartment(ApartmentCreateRequest request, Guid landlordUid, List<string> imageUrls)
    {
        LandlordUid = landlordUid;
        Price = request.Price;
        Title = request.Title;
        Description = request.Description;

        ProvinceDivisionUid = request.ProvinceDivisionUid;
        WardDivisionUid     = request.WardDivisionUid;
        MetaData            = request.MetaData;

        AreaLength = request.AreaLength;
        AreaWidth  = request.AreaWidth;
        Type       = request.Type;
        Status     = request.Status;
        Images     = imageUrls;
    }

    public Apartment(Apartment existing, ApartmentCreateRequest request, List<string>? imageUrls = null)
    {
        existing.Price = request.Price;
        existing.Title = request.Title;
        existing.Description = request.Description;

        existing.ProvinceDivisionUid = request.ProvinceDivisionUid;
        existing.WardDivisionUid     = request.WardDivisionUid;
        existing.MetaData            = request.MetaData;

        existing.AreaLength = request.AreaLength;
        existing.AreaWidth  = request.AreaWidth;
        existing.Type       = request.Type;
        existing.Status     = request.Status;

        if (imageUrls != null && imageUrls.Count > 0)
            existing.Images = imageUrls;
    }
}
