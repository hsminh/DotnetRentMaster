using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace RentMaster.Management.RealEstate.Types.Request
{
    public class ApartmentRoomCreateRequest
    {
        [Required]
        public Guid ApartmentUid { get; set; }
        
        [MaxLength(50)]
        public string RoomNumber { get; set; } = string.Empty;

        [Required]
        [Range(0, double.MaxValue)]
        public decimal? Price { get; set; }
        
        [Required]
        [Range(0, double.MaxValue)]
        public decimal? AreaLength { get; set; }
        
        [Required]
        [Range(0, double.MaxValue)]
        public decimal? AreaWidth { get; set; }

        [MaxLength(50)]
        public string Status { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Client gửi JSON string, server parse sang Dictionary
        /// </summary>
        public string? MetaDataJson { get; set; }

        [NotMapped]
        public Dictionary<string,string> MetaData
        {
            get
            {
                if (string.IsNullOrWhiteSpace(MetaDataJson))
                    return new Dictionary<string,string>();
                try
                {
                    return JsonSerializer.Deserialize<Dictionary<string,string>>(MetaDataJson!) ?? new Dictionary<string,string>();
                }
                catch
                {
                    return new Dictionary<string,string>();
                }
            }
        }

        // Multiple image files for the room
        public List<IFormFile>? Files { get; set; } = new();
    }
}