namespace RentMaster.Management.ConsumerContact.Request
{
    public class ConsumerContactRequest
    {
        public decimal Price { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public decimal AreaLength { get; set; }

        public decimal AreaWidth { get; set; }


    }
}