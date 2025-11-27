using RentMaster.Addresses.Services;

namespace RentMaster.Addresses.Interface
{
    public interface IAddressImport
    {
        Task<ImportResult> ImportFromCsvAsync(IFormFile file);
        Task<ImportResult> ImportFromRecordsAsync(List<AddressCsvRecord> records);
    }
}