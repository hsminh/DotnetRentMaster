using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RentMaster.Core.Models;
using RentMaster.Management.RealEstate.Models;

namespace RentMaster.Management.ConsumerFavorite.Models;

[Table("consumer_favorites")]
public class ConsumerFavorite : BaseModel
{
    [Required]
    [MaxLength(20)]
    public string Type { get; set; }
    
    public Apartment? Apartment { get; set; } 
    public ApartmentRoom? ApartmentRoom { get; set; } 
    
    // Navigation property IDs
    public Guid? ApartmentUid { get; set; }
    public Guid? ApartmentRoomUid { get; set; }
    
    [Required]
    public Guid consumer_id { get; set; }
    
    [ForeignKey("consumer_id")]
    public Accounts.Models.Consumer? Consumer { get; set; }

}