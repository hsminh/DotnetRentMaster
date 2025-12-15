using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RentMaster.Core.Models;

namespace RentMaster.Management.RentalContract.Models;

[Table("rental_contract_monthly_payments")]
public class RentalContractMonthlyPayment : BaseModel
{
    [Required]
    public Guid RentalContractUid { get; set; }

    [ForeignKey(nameof(RentalContractUid))]
    public RentalContract RentalContract { get; set; } = null!;

    [Required]
    public int Year { get; set; }

    [Required]
    public int Month { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    [Required]
    public bool IsPaid { get; set; } = false;

    public DateTime? PaidAt { get; set; }

    public Guid? CollectedByUid { get; set; }

    public string? Note { get; set; }

    public string? Method { get; set; }
}
