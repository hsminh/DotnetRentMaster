using RentMaster.Management.RentalContract.Types.Enums;

namespace RentMaster.Management.RentalContract.Types.Request;

public class RentalContractUpdateRequest
{
    public string? Type { get; set; }

    public Guid? ResponsibleUid { get; set; }

    public string? ParticipantUidsJson { get; set; }

    public decimal? MonthlyPrice { get; set; }

    public decimal? DepositAmount { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public ContractStatus? Status { get; set; }
}
