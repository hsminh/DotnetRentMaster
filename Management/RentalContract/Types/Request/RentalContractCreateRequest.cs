using System.ComponentModel.DataAnnotations;

namespace RentMaster.Management.RentalContract.Types.Request;

public class RentalContractCreateRequest
{
    [Required]
    public Guid ConsumerUid { get; set; }

    [Required]
    public Guid LandlordUid { get; set; }

    [Required]
    public Guid ApartmentUid { get; set; }

    [Required]
    public string Type { get; set; }

    [Required]
    public Guid ResponsibleUid { get; set; }

    public string? ParticipantUidsJson { get; set; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal MonthlyPrice { get; set; }

    [Range(0, double.MaxValue)]
    public decimal DepositAmount { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }
}
