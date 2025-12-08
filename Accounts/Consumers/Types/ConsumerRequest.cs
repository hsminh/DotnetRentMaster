using System.ComponentModel.DataAnnotations;

namespace RentMaster.Accounts.Consumers.Types;

public class ConsumerRequest
{
    [Required(ErrorMessage = "Tên là bắt buộc.")]
    public string? FirstName { get; set; }
    
    [Required(ErrorMessage = "Họ là bắt buộc.")]
    public string? LastName { get; set; }
    
    [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
    public string? PhoneNumber { get; set; }
    
    [MinLength(6, ErrorMessage = "Mật khẩu mới phải có ít nhất 6 ký tự.")]
    public string? Password { get; set; }
    
    public IFormFile? Avatar { get; set; }
}