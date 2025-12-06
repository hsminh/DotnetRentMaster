using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RentMaster.Accounts.LandLords.Models;
using RentMaster.Management.ConsumerContact.enums;
using RentMaster.Management.RealEstate.Models;

namespace RentMaster.Management.ConsumerContact.Models
{
    public class ConsumerContact
    {
        [Key]
        public Guid Uid { get; set; } = Guid.NewGuid();
        
        [Required]
        public Guid Consumer_Uid { get; set; }
        
        [Required]
        public Guid Landlord_Uid { get; set; }
        
        [Required]
        public Guid Apartment_UID { get; set; }
        
        [Required]
        [Column(TypeName = "varchar(20)")]
        public JoinApartmentStatus Status { get; set; } = JoinApartmentStatus.Pending;
        
        [Required]
        [MaxLength(20)]
        public string Type { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [ForeignKey("Consumer_Uid")]
        public virtual Accounts.Models.Consumer Consumer { get; set; }
        
        [ForeignKey("Landlord_Uid")]
        public virtual LandLord LandLord { get; set; }
        
        [NotMapped]
        public virtual Apartment? Apartment { get; set; } 
        
        [NotMapped]
        public virtual ApartmentRoom? ApartmentRoom { get; set; }
    }
}
