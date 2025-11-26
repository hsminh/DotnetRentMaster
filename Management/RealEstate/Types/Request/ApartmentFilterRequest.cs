namespace RentMaster.Management.RealEstate.Types.Request
{
    public class ApartmentFilterRequest
    {
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public Guid? WardDivisionUid { get; set; }
        public Guid? ProvinceDivisionUid { get; set; }
        public string? ProvinceName { get; set; }
    }

    public class RoomFilterRequest
    {
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public Guid? WardDivisionUid { get; set; }
        public Guid? ProvinceDivisionUid { get; set; }
        public string? ProvinceName { get; set; }
        public string? AddressDetail { get; set; } 
        public Guid? ApartmentUid { get; set; } 
    }
}