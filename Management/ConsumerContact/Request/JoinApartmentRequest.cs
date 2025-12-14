using System.ComponentModel.DataAnnotations;

namespace RentMaster.Management.ConsumerContact.Request
{
    public class JoinApartmentRequest
    {
        
        [Required]
        public Guid LandlordUid { get; set; }
        
        [Required]
        public Guid ApartmentUid { get; set; }
        
        [Required]
        public string Type { get; set; } = string.Empty;
    }
}
