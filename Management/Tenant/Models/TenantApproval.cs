using RentMaster.Accounts.LandLords.Models;
using RentMaster.Management.RealEstate.Models;

namespace RentMaster.Management.Tenant.Models;
using RentMaster.Core.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


[Table("tenant_approvals")]
public class TenantApproval : BaseModel
{
    [Required]
    public Guid ConsumerUid { get; set; }

    [ForeignKey(nameof(ConsumerUid))]
    public virtual Accounts.Models.Consumer Consumer { get; set; } = null!;

    [Required]
    public Guid LandlordUid { get; set; }

    [ForeignKey(nameof(LandlordUid))]
    public virtual LandLord Landlord { get; set; } = null!;

    [Required]
    public Guid OwnerUid { get; set; }

    [Required]
    [MaxLength(20)]
    [Column(TypeName = "varchar(20)")]
    public string OwnerType { get; set; } = string.Empty;

    public virtual Apartment? Apartment { get; set; }
    public virtual ApartmentRoom? ApartmentRoom { get; set; }

    [Required]
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ActionAt { get; set; }

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "pending";


    [NotMapped]
    public bool IsPending => Status == "pending";
    [NotMapped]
    public bool IsApproved => Status == "approved";
    [NotMapped]
    public bool IsRejected => Status == "rejected";
}