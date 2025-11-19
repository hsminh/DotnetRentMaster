using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentMaster.Management.RealEstate.Types.Request
{
    public class ApartmentRoomCreateRequest
    {
        [Required]
        public Guid ApartmentUid { get; set; }
        
        [MaxLength(50)]
        public string RoomNumber { get; set; } = string.Empty;
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal? Price { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal? AreaLength { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal? AreaWidth { get; set; }
        
        [MaxLength(50)]
        public string Status { get; set; } = string.Empty;
        
        public string Description { get; set; } = string.Empty;
        
        public Guid? ProvinceDivisionUid { get; set; }

        public Guid? WardDivisionUid { get; set; }

        public string? MetaData { get; set; }
        // Multiple image files for the room
        public List<IFormFile>? Files { get; set; } = new List<IFormFile>();
    }
}
