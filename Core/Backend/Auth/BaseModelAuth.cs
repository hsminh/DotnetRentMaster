using System.ComponentModel.DataAnnotations;
using RentMaster.Core.Models;
using RentMaster.Core.types.enums;

namespace RentMaster.Core.Backend.Auth;

public abstract class BaseAuth : BaseModel
{
    [Required]
    [EmailAddress]
    public string Gmail { get; set; } = string.Empty;

    public string Scope { get; set; } = string.Empty;
    
    [Required]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
    public string Password { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;
    
    public string Status { get; set; } = UserStatus.Active.ToString();

    [Phone]
    public string? PhoneNumber { get; set; }
}