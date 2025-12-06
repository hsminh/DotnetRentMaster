using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using RentMaster.Core.Models;
using RentMaster.Core.types.enums;

namespace RentMaster.Core.Backend.Auth;

public abstract class BaseAuth : BaseModel
{
    [Required]
    [EmailAddress]
    public string Gmail { get; set; } = string.Empty;
    
    public string Password { get; set; } = string.Empty;

    
    [JsonPropertyName("Status")]
    public string Status { get; set; } = UserStatus.Active.ToString();
    
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    [Phone]
    public string? PhoneNumber { get; set; }
    
    public string? Avatar { get; set; }
    
    public bool IsVerified { get; set; } = false;
}