using Microsoft.AspNetCore.Http;

namespace RentMaster.RealEstate.Types.Request
{
    public class ApartmentCreateRequest
    {
        public decimal Price { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Guid AddressDivisionUid { get; set; }
        public decimal AreaLength { get; set; }
        public decimal AreaWidth { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;

        // Multiple image files
        public List<IFormFile>? Files { get; set; } = new List<IFormFile>();
    }
}