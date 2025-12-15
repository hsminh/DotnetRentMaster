using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RentMaster.Core.Models;
using RentMaster.Management.RentalContract.Types.Enums;

namespace RentMaster.Management.RentalContract.Models;

[Table("rental_contracts")]
public class RentalContract : BaseModel
{

    [Required]
    public Guid LandlordUid { get; set; }

    [Required]
    public Guid ApartmentUid { get; set; }

    public String Type { get; set; }
    
    [Required]
    public Guid ResponsibleUid { get; set; }
    
    public List<Guid>? ParticipantUids { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal MonthlyPrice { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal DepositAmount { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    [Required]
    [Column(TypeName = "varchar(20)")]
    public ContractStatus Status { get; set; } = ContractStatus.Active;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
