using System.ComponentModel.DataAnnotations;

namespace RentMaster.Management.ConsumerFavorite.Models.DTOs;

public class CreateConsumerFavoriteDto
{
    [Required]
    [MaxLength(20)]
    public string Type { get; set; }
    
    public Guid? ApartmentUid { get; set; }
    
    public Guid? ApartmentRoomUid { get; set; }
    
    public Guid ConsumerId { get; set; }
}
