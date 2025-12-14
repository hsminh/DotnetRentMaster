using System.Text.Json.Serialization;

namespace RentMaster.Addresses.DTO
{
    public enum DivisionTypeEnum
    {
        Country,
        province,
        ward,
        street
    }
    public class CreateAddressDto
    {
        public string Name { get; set; } = string.Empty;
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public DivisionTypeEnum Type { get; set; } = DivisionTypeEnum.province;
        public string? ParentId { get; set; }
        public string Code { get; set; } = string.Empty;
    }
}