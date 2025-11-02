using RentMaster.Core.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace RentMaster.RealEstate.apartment_rooms.Models;

[Table("apartment_rooms")]
public class ApartmentRoom : BaseModel
{
    [Required]
    public Guid ApartmentUid { get; set; }  

    [MaxLength(50)]
    public string RoomNumber { get; set; } 

    [Column(TypeName = "decimal(18,2)")]
    public decimal? Price { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? AreaLength { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? AreaWidth { get; set; }

    public int? FloorNumber { get; set; }

    [MaxLength(50)]
    [EnumDataType(typeof(ApartmentStatus))]
    [Column(TypeName = "varchar(50)")]
    public string Status { get; set; }

    public string Description { get; set; }
}