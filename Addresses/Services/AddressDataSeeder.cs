using System;
using System.IO;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RentMaster.Addresses.Data;
using RentMaster.Addresses.Models;
using RentMaster.Data;

namespace RentMaster.Addresses.Services
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

        public async Task SeedAsync(string filePath = "Data/address_data.csv")
        {
            try
            {
                _logger.LogInformation("Starting address data import...");
                
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                    Delimiter = ",",
                    HeaderValidated = null,
                    MissingFieldFound = null
                };

                using var reader = new StreamReader(filePath);
                using var csv = new CsvReader(reader, config);
                
                // Register the mapping
                csv.Context.RegisterClassMap<AddressCsvMap>();
                
                // Read all records
                var records = csv.GetRecords<AddressCsvRecord>().ToList();

                // Process provinces first
                var provinces = records.Where(r => r.Type == "2").ToList();
                // Process wards
                var wards = records.Where(r => r.Type == "4").ToList();

                var stats = new ImportStats();
                var country = await GetOrCreateVietnamAsync();

                // Process provinces
                foreach (var provinceRecord in provinces)
                {
                    var province = new AddressDivision
                    {
                        Name = CleanProvinceName(provinceRecord.Name),
                        Code = provinceRecord.Code,
                        Type = DivisionType.Province,
                        ParentId = country.Id,
                        IsDeprecated = false,
                        PreviousUnitCodes = GetPreviousUnitCodes(provinceRecord.OldCode)
                    };

                    var existing = await _context.AddressDivisions
                        .FirstOrDefaultAsync(x => x.Code == province.Code && x.Type == DivisionType.Province);

                    if (existing == null)
                    {
                        await _context.AddressDivisions.AddAsync(province);
                        stats.ProvincesCreated++;
                        _logger.LogInformation("Created province: {ProvinceName}", province.Name);
                    }
                    else
                    {
                        // Update existing
                        existing.Name = province.Name;
                        existing.PreviousUnitCodes = province.PreviousUnitCodes;
                        _context.AddressDivisions.Update(existing);
                        stats.ProvincesUpdated++;
                        if (stats.ProvincesUpdated % 10 == 0)
                        {
                            _logger.LogInformation("Updated province: {ProvinceName}", province.Name);
                        }
                    }
                }

                await _context.SaveChangesAsync();

                // Process wards
                foreach (var wardRecord in wards)
                {
                    var province = await _context.AddressDivisions
                        .FirstOrDefaultAsync(x => x.Code == wardRecord.ParentCode && x.Type == DivisionType.Province);

                    if (province == null)
                    {
                        _logger.LogWarning("Parent province not found for ward: {WardCode} - {WardName}", 
                            wardRecord.Code, wardRecord.Name);
                        continue;
                    }

                    var ward = new AddressDivision
                    {
                        Name = wardRecord.Name,
                        Code = wardRecord.Code,
                        Type = DivisionType.Ward,
                        ParentId = province.Id,
                        IsDeprecated = false,
                        PreviousUnitCodes = GetPreviousUnitCodes(wardRecord.OldCode)
                    };

                    var existing = await _context.AddressDivisions
                        .FirstOrDefaultAsync(x => x.Code == ward.Code && x.Type == DivisionType.Ward);

                    if (existing == null)
                    {
                        await _context.AddressDivisions.AddAsync(ward);
                        stats.WardsCreated++;
                    }
                    else
                    {
                        // Update existing
                        existing.Name = ward.Name;
                        existing.ParentId = ward.ParentId;
                        existing.PreviousUnitCodes = ward.PreviousUnitCodes;
                        _context.AddressDivisions.Update(existing);
                        stats.WardsUpdated++;
                    }

                    // Save in batches to improve performance
                    if ((stats.WardsCreated + stats.WardsUpdated) % 100 == 0)
                    {
                        await _context.SaveChangesAsync();
                    }
                }

                // Save any remaining changes
                await _context.SaveChangesAsync();

                _logger.LogInformation("""
                    Address import completed:
                    - Provinces: {Created} created, {Updated} updated
                    - Wards: {WardsCreated} created, {WardsUpdated} updated
                    """, 
                    stats.ProvincesCreated, stats.ProvincesUpdated,
                    stats.WardsCreated, stats.WardsUpdated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing address data");
                throw;
            }
        }

        private async Task<AddressDivision> GetOrCreateVietnamAsync()
        {
            var vietnam = await _context.AddressDivisions
                .FirstOrDefaultAsync(x => x.Code == "VN" && x.Type == DivisionType.Country);

            if (vietnam == null)
            {
                vietnam = new AddressDivision
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Việt Nam",
                    Code = "VN",
                    Type = DivisionType.Country,
                    IsDeprecated = false
                };
                await _context.AddressDivisions.AddAsync(vietnam);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Created Vietnam country record");
            }

            return vietnam;
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

        private static List<string> GetPreviousUnitCodes(string oldCode)
        {
            if (string.IsNullOrWhiteSpace(oldCode))
                return new List<string>();

            return oldCode
                .Split(';', StringSplitOptions.RemoveEmptyEntries)
                .Select(code => code.Trim())
                .Where(code => !string.IsNullOrEmpty(code))
                .ToList();
        }

        private class ImportStats
        {
            public int ProvincesCreated { get; set; }
            public int ProvincesUpdated { get; set; }
            public int WardsCreated { get; set; }
            public int WardsUpdated { get; set; }
        }
    }
}
