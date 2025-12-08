using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentMaster.Core.Models
{
    public class PaymentTransaction
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [Required]
        [MaxLength(100)]
        public string OrderId { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(50)]
        public string PartnerCode { get; set; } = string.Empty;
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
        
        [MaxLength(500)]
        public string? Message { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(50)]
        public string PayType { get; set; } = string.Empty;
        
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
