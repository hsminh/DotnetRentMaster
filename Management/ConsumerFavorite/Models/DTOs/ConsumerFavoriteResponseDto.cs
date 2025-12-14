using RentMaster.Management.RealEstate.Models;

namespace RentMaster.Management.ConsumerFavorite.Models.DTOs;

public class ConsumerFavoriteResponseDto
{
    public string Type { get; set; }
    public Apartment? Apartment { get; set; }
    public ApartmentRoom? ApartmentRoom { get; set; }
    public Guid? ApartmentUid { get; set; }
    public Guid? ApartmentRoomUid { get; set; }
    public Guid ConsumerId { get; set; }
    public Guid Uid { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDelete { get; set; }
    
    // Add any other properties you want to include in the response
    
    public static ConsumerFavoriteResponseDto FromEntity(Models.ConsumerFavorite entity)
    {
        return new ConsumerFavoriteResponseDto
        {
            Type = entity.Type,
            Apartment = entity.Apartment,
            ApartmentRoom = entity.ApartmentRoom,
            ApartmentUid = entity.ApartmentUid,
            ApartmentRoomUid = entity.ApartmentRoomUid,
            ConsumerId = entity.consumer_id,
            Uid = entity.Uid,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            IsDelete = entity.IsDelete
        };
    }
}
