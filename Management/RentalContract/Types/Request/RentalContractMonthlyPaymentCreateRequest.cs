using System.ComponentModel.DataAnnotations;

namespace RentMaster.Management.RentalContract.Types.Request;

public class RentalContractMonthlyPaymentCreateRequest
{
    [Required]
    public Guid RentalContractUid { get; set; }

    [Required]
    [Range(1900, 2100)]
    public int Year { get; set; }

    [Required]
    [Range(1, 12)]
    public int Month { get; set; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    public string? Note { get; set; }

    public string? Method { get; set; }
}
