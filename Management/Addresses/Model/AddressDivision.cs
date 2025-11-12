using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentMaster.Management.AddressDivision.Models;

[Table("address_divisions")]
    public class AddressDivision  
    {
        [Key]
        [Column("code")]
        public string Code { get; set; } = null!;
        [Column("name")]
        public string Name { get; set; } = null!;
        [Column("parent_code")]
        public string? ParentCode { get; set; }
        
        [Column("type")]
        public int Type { get; set; }
        [Column("old_code")]
        public string? OldCode { get; set; }
    }
