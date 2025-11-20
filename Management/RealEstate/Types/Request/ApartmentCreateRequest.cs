using Microsoft.AspNetCore.Http;
using System;

namespace RentMaster.Management.RealEstate.Types.Request
{
    public class ApartmentCreateRequest
    {
        public override string ToString()
        {
            return $"Price: {Price}, Title: {Title}, Description: {Description}, " +
                   $"AreaLength: {AreaLength}, AreaWidth: {AreaWidth}, Type: {Type}, " +
                   $"Status: {Status}, ProvinceDivisionUid: {ProvinceDivisionUid}, " +
                   $"WardDivisionUid: {WardDivisionUid}, MetaData: {MetaData}, " +
                   $"FilesCount: {Files?.Count ?? 0}";
        }
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