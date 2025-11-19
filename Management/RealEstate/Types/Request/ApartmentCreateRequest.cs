using Microsoft.AspNetCore.Http;

namespace RentMaster.Management.RealEstate.Types.Request
{
    public class ApartmentCreateRequest
    {
        public decimal Price { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public decimal AreaLength { get; set; }

        public decimal AreaWidth { get; set; }

        public string Type { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public Guid ProvinceDivisionUid { get; set; }

        public Guid WardDivisionUid { get; set; }

        public string? MetaData { get; set; }

        public List<IFormFile>? Files { get; set; } = new List<IFormFile>();
    }
}