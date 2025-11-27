using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using RentMaster.Addresses.Services;

namespace RentMaster.Addresses.Commands
{
    public class AddressDataSeeder
    {
        private readonly ILogger<AddressDataSeeder> _logger;
        private readonly IAddressImportService _addressImportService;

        public AddressDataSeeder(
            ILogger<AddressDataSeeder> logger,
            IAddressImportService addressImportService)
        {
            _logger = logger;
            _addressImportService = addressImportService;
        }

        public async Task SeedAsync(string filePath = "Addresses/Data/address_data.csv")
        {
            try
            {
                _logger.LogInformation("Starting address data seed from file {FilePath}...", filePath);

                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                    Delimiter = ",",
                    HeaderValidated = null,
                    MissingFieldFound = null
                };

                using var reader = new StreamReader(filePath);
                using var csv = new CsvReader(reader, config);
                csv.Context.RegisterClassMap<AddressCsvMap>();
                var records = csv.GetRecords<AddressCsvRecord>().ToList();

                var result = await _addressImportService.ImportFromRecordsAsync(records);

                if (!result.Success)
                {
                    _logger.LogError("Seed failed: {Message}", result.Message);
                }
                else
                {
                    _logger.LogInformation("Seed completed: {ImportedCount} records", result.ImportedCount);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding address data");
                throw;
            }
        }
    }
}
