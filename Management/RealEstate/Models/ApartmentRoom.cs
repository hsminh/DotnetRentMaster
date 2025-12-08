using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RentMaster.Core.Models;
using RentMaster.Management.RealEstate.Types.Request;

namespace RentMaster.Management.RealEstate.Models
{
    [Table("apartment_rooms")]
    public class ApartmentRoom : BaseModel
    {
        [Required]
        public Guid ApartmentUid { get; set; }  

        [Required]
        public Guid LandlordUid { get; set; }

        [MaxLength(50)]
        public string RoomNumber { get; set; } = "#1";

        [Column(TypeName = "decimal(18,2)")]
        public decimal? Price { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? AreaLength { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? AreaWidth { get; set; }

        [Column(TypeName = "jsonb")]
        public Dictionary<string,string> MetaData { get; set; } = new();

        [MaxLength(50)]
        [Column(TypeName = "varchar(50)")]
        public string Status { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public List<string> Images { get; set; } = new();

        public ApartmentRoom() {}

        public ApartmentRoom(ApartmentRoomCreateRequest request, Guid landlordUid, Guid apartmentUid, List<string>? imageUrls = null)
        {
            LandlordUid = landlordUid;
            ApartmentUid = apartmentUid;
            RoomNumber = string.IsNullOrWhiteSpace(request.RoomNumber) ? "#1" : request.RoomNumber;
            Price = request.Price;
            AreaLength = request.AreaLength;
            AreaWidth = request.AreaWidth;
            Status = request.Status;
            Description = request.Description;

            MetaData = request.MetaData; 

            if (imageUrls != null && imageUrls.Count > 0)
                Images = imageUrls;
        }

        public void UpdateFromRequest(ApartmentRoomCreateRequest request, List<string>? imageUrls = null)
        {
            RoomNumber = string.IsNullOrWhiteSpace(request.RoomNumber) ? RoomNumber : request.RoomNumber;
            Price = request.Price;
            AreaLength = request.AreaLength;
            AreaWidth = request.AreaWidth;
            Status = request.Status;
            Description = request.Description;
            MetaData = request.MetaData;

            if (imageUrls != null && imageUrls.Count > 0)
                Images = imageUrls;
        }
    }
}
