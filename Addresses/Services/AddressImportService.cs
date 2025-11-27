using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using RentMaster.Addresses.Models;
using RentMaster.Data;

namespace RentMaster.Addresses.Services
{


    public class AddressImportService : IAddressImportService
    {
        private readonly ILogger<AddressImportService> _logger;
        private readonly AppDbContext _context;

        public AddressImportService(
            ILogger<AddressImportService> logger,
            AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<ImportResult> ImportFromCsvAsync(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return new ImportResult { Success = false, Message = "No file uploaded" };

                if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                    return new ImportResult { Success = false, Message = "Only CSV files are allowed" };

                using var stream = new StreamReader(file.OpenReadStream());
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                    Delimiter = ",",
                    HeaderValidated = null,
                    MissingFieldFound = null
                };

                using var csv = new CsvReader(stream, config);
                csv.Context.RegisterClassMap<AddressCsvMap>();
                var records = csv.GetRecords<AddressCsvRecord>().ToList();

                if (!records.Any())
                    return new ImportResult { Success = false, Message = "No records found in the file" };

                return await ImportFromRecordsAsync(records);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing address data from CSV");
                return new ImportResult { Success = false, Message = $"Error importing data: {ex.Message}" };
            }
        }

        public async Task<ImportResult> ImportFromRecordsAsync(List<AddressCsvRecord> records)
        {
            try
            {
                _logger.LogInformation("Starting address data import from records...");

                await _context.Database.EnsureCreatedAsync();

                // Nếu muốn chặn import khi đã có data:
                // if (await _context.AddressDivisions.AnyAsync())
                // {
                //     return new ImportResult
                //     {
                //         Success = false,
                //         Message = "Address data already exists. Import is skipped."
                //     };
                // }

                var divisions = new List<AddressDivision>();

                // Root node: Vietnam
                var vietnam = new AddressDivision
                {
                    Name = "Việt Nam",
                    Code = "VN",
                    Type = DivisionType.Country,
                    ParentId = null,
                    IsDeprecated = false
                };
                divisions.Add(vietnam);

                // Provinces
                var provinces = records
                    .Where(r => r.Type == "2")
                    .Select(r => new AddressDivision
                    {
                        Name = CleanProvinceName(r.Name),
                        Code = r.Code,
                        Type = DivisionType.Province,
                        ParentId = vietnam.Uid.ToString(),
                        IsDeprecated = false,
                        PreviousUnitCodes = GetPreviousUnitCodes(r.OldCode)
                    })
                    .ToList();
                divisions.AddRange(provinces);

                var provinceDict = provinces.ToDictionary(p => p.Code);

                // Wards (type "3" or "4")
                var wards = records
                    .Where(r => r.Type == "3" || r.Type == "4")
                    .Select(r =>
                    {
                        string? parentUid = null;

                        if (provinceDict.ContainsKey(r.ParentCode))
                        {
                            parentUid = provinceDict[r.ParentCode].Uid.ToString();
                        }
                        else
                        {
                            var districtRecord = records.FirstOrDefault(d => d.Code == r.ParentCode);
                            if (districtRecord != null && provinceDict.ContainsKey(districtRecord.ParentCode))
                                parentUid = provinceDict[districtRecord.ParentCode].Uid.ToString();
                        }

                        return new AddressDivision
                        {
                            Name = r.Name,
                            Code = r.Code,
                            Type = DivisionType.Ward,
                            ParentId = parentUid,
                            IsDeprecated = false,
                            PreviousUnitCodes = GetPreviousUnitCodes(r.OldCode)
                        };
                    })
                    .ToList();
                divisions.AddRange(wards);

                // Create a dictionary to look up wards by their code
                var wardDict = wards.ToDictionary(w => w.Code, w => w);

                // Streets (type "5")
                var streets = records
                    .Where(r => r.Type == "5")
                    .Select(r =>
                    {
                        string? parentUid = null;

                        // Try to find parent ward by parent code
                        if (wardDict.TryGetValue(r.ParentCode, out var ward))
                        {
                            parentUid = ward.Uid.ToString();
                        }

                        return new AddressDivision
                        {
                            Name = r.Name,
                            Code = r.Code,
                            Type = DivisionType.Street,
                            ParentId = parentUid,
                            IsDeprecated = false,
                            PreviousUnitCodes = GetPreviousUnitCodes(r.OldCode)
                        };
                    })
                    .Where(s => s.ParentId != null) // Only include streets with valid parent wards
                    .ToList();
                
                divisions.AddRange(streets);

                // Batch insert
                const int batchSize = 100;
                for (int i = 0; i < divisions.Count; i += batchSize)
                {
                    var batch = divisions.Skip(i).Take(batchSize);
                    await _context.AddressDivisions.AddRangeAsync(batch);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Imported {Count} records...", Math.Min(batchSize, divisions.Count - i));
                }

                _logger.LogInformation("Successfully imported {Count} address records", divisions.Count);

                return new ImportResult
                {
                    Success = true,
                    Message = "Address data imported successfully",
                    ImportedCount = divisions.Count
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing address data from records");
                return new ImportResult
                {
                    Success = false,
                    Message = $"Error importing data: {ex.Message}"
                };
            }
        }

        private static string CleanProvinceName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return name;

            name = name.Trim();

            if (name.Equals("Thành phố Hồ Chí Minh", StringComparison.OrdinalIgnoreCase))
                return "Thành Phố Hồ Chí Minh";

            var prefixes = new[] { "Thành phố ", "Tỉnh ", "Thành Phố " };
            foreach (var prefix in prefixes)
            {
                if (name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    return name[prefix.Length..].Trim();
                }
            }

            return name;
        }

        private static List<string> GetPreviousUnitCodes(string? oldCode)
        {
            if (string.IsNullOrWhiteSpace(oldCode))
                return new List<string>();

            return oldCode
                .Split(';', StringSplitOptions.RemoveEmptyEntries)
                .Select(code => code.Trim())
                .Where(code => !string.IsNullOrEmpty(code))
                .ToList();
        }
    }

    // GIỮ lại record + map ở đây luôn cho gọn (hoặc để trong namespace Commands cũng được)
    public class AddressCsvRecord
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ParentCode { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string? OldCode { get; set; }
    }

    public sealed class AddressCsvMap : ClassMap<AddressCsvRecord>
    {
        public AddressCsvMap()
        {
            Map(m => m.Code).Name("code");
            Map(m => m.Name).Name("name");
            Map(m => m.ParentCode).Name("parent_code");
            Map(m => m.Type).Name("type");
            Map(m => m.OldCode).Name("old_code");
        }
    }
}
