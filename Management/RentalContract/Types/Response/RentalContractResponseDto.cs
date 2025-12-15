namespace RentMaster.Management.RentalContract.Types.Response;

public class RentalContractResponseDto
{
    public Guid Uid { get; set; }

    public Guid ConsumerUid { get; set; }

    public Guid LandlordUid { get; set; }

    public Guid ApartmentUid { get; set; }

    public string? Type { get; set; }

    public Guid ResponsibleUid { get; set; }

    public List<Guid>? ParticipantUids { get; set; }

    public decimal MonthlyPrice { get; set; }

    public decimal DepositAmount { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public object? ApartmentDetails { get; set; }
}
