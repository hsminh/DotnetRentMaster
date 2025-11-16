using System.ComponentModel.DataAnnotations.Schema;
using RentMaster.Core.Models;

namespace RentMaster.Addresses.Models
{
    [Table("address_division")]
    public class AddressDivision : BaseModel
    {
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;

        public string Type { get; set; } = DivisionType.Province;

        public string? ParentId { get; set; }

        public bool IsDeprecated { get; set; }
        public DateTime? DeprecatedAt { get; set; }

        public List<string> PreviousUnitCodes { get; set; } = new List<string>();
    }

    public static class DivisionType
    {
        public const string Province = "province";
        public const string Ward = "ward";
        public const string Country = "country";
    }
}