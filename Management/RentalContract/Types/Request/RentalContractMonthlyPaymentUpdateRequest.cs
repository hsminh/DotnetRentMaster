namespace RentMaster.Management.RentalContract.Types.Request;

public class RentalContractMonthlyPaymentUpdateRequest
{
    public bool? IsPaid { get; set; }

    public DateTime? PaidAt { get; set; }

    public Guid? CollectedByUid { get; set; }

    public string? Note { get; set; }

    public string? Method { get; set; }
}
