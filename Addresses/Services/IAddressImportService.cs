namespace RentMaster.Addresses.Services
{
    public interface IAddressImportService
    {
        Task<ImportResult> ImportFromCsvAsync(IFormFile file);
        Task<ImportResult> ImportFromRecordsAsync(List<AddressCsvRecord> records);
    }

    public class ImportResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int ImportedCount { get; set; }
    }
}
