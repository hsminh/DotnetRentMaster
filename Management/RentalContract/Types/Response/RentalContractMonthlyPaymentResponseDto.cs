namespace RentMaster.Management.RentalContract.Types.Response;

public class RentalContractMonthlyPaymentResponseDto
{
    public Guid Uid { get; set; }

    public Guid RentalContractUid { get; set; }

    public int Year { get; set; }

    public int Month { get; set; }

    public decimal Amount { get; set; }

    public bool IsPaid { get; set; }

    public DateTime? PaidAt { get; set; }

    public Guid? CollectedByUid { get; set; }

    public string? Note { get; set; }

    public string? Method { get; set; }

    public DateTime CreatedAt { get; set; }
}
