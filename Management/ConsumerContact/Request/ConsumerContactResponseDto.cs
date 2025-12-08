namespace RentMaster.Management.ConsumerContact.Request;

public class ConsumerContactResponseDto
{
    public Guid Uid { get; set; }
    public string Status { get; set; }
    public string Type { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    
    public Accounts.Models.Consumer? Consumer { get; set; }
    
    public object? RealEstateUnit { get; set; }
}