using RentMaster.Core.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RentMaster.RealEstate.Models;

namespace RentMaster.Management.Tenant.Models;

[Table("tenants")]
public class Tenant : BaseModel
{
    [Required]
    public Guid UserUid { get; set; } 

    [ForeignKey(nameof(UserUid))]
    public virtual Accounts.Models.Consumer User { get; set; } = null!;

    [Required]
    public Guid OwnerUid { get; set; }

    [Required]
    [MaxLength(20)]
    [Column(TypeName = "varchar(20)")]
    public string OwnerType { get; set; } = string.Empty;

    [NotMapped]
    public object? Owner => OwnerType switch
    {
        "apartment" => Apartment,
        "room" => ApartmentRoom,
        _ => null
    };

    public virtual Apartment? Apartment { get; set; }
    public virtual ApartmentRoom? ApartmentRoom { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? DepositAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? MonthlyRent { get; set; }

    [MaxLength(50)]
    public string Status { get; set; } = "active"; 

    public string? Note { get; set; }

    [NotMapped]
    public bool IsValid => OwnerType == "apartment" || OwnerType == "room";
}