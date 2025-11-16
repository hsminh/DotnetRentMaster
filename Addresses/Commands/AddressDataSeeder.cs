using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using RentMaster.Addresses.Models;
using RentMaster.Data;

namespace RentMaster.Addresses.Commands
{
    public class AddressDataSeeder
    {
        private readonly ILogger<AddressDataSeeder> _logger;
        private readonly AppDbContext _context;

        public AddressDataSeeder(ILogger<AddressDataSeeder> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task SeedAsync(string filePath = "Addresses/Data/address_data.csv")
        {
            try
            {
                _logger.LogInformation("Starting address data import...");

                // Đảm bảo DB đã tạo
                await _context.Database.EnsureCreatedAsync();

                if (await _context.AddressDivisions.AnyAsync())
                {
                    _logger.LogInformation("Address data already exists. Skipping import.");
                    return;
                }

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

                // Subdivisions (districts/wards)
                var subDivisions = records
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
                divisions.AddRange(subDivisions);

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
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing address data");
                throw;
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

