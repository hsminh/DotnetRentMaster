using Microsoft.AspNetCore.Mvc;
using RentMaster.Data;
using RentMaster.Management.AddressDivision.Models;
using CsvHelper;
using System.Globalization;

namespace AddressImportApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AddressImportController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AddressImportController(AppDbContext context)
        {
            _context = context;
        }

     [HttpPost("import")]
public async Task<IActionResult> ImportCsv([FromForm] IFormFile file)
{
    if (file == null || file.Length == 0)
        return BadRequest("File CSV không hợp lệ");

    using var stream = new StreamReader(file.OpenReadStream());
    using var csv = new CsvReader(stream, CultureInfo.InvariantCulture);

    var records = csv.GetRecords<dynamic>().ToList();
    int count = 0;

    // 1. Insert các record không có parent trước (Tỉnh/Thành phố)
    var roots = records.Where(r => string.IsNullOrEmpty((string)r.parent_code)).ToList();
    foreach (var row in roots)
    {
        var code = (string)row.code;
        if (!_context.AddressDivisions.Any(a => a.Code == code))
        {
            _context.AddressDivisions.Add(new AddressDivision
            {
                Code = code,
                Name = (string)row.name,
                ParentCode = null,
                Type = int.Parse((string)row.type),
                OldCode = (string?)row.old_code
            });
            count++;
        }
    }
    await _context.SaveChangesAsync();

    // 2. Insert các record có parent (Huyện, Xã...)
    var children = records.Where(r => !string.IsNullOrEmpty((string)r.parent_code)).ToList();

    foreach (var row in children)
    {
        var code = (string)row.code;
        var parentCode = (string)row.parent_code;

        // Kiểm tra parent đã tồn tại
        if (!_context.AddressDivisions.Any(a => a.Code == code) &&
            _context.AddressDivisions.Any(a => a.Code == parentCode))
        {
            _context.AddressDivisions.Add(new AddressDivision
            {
                Code = code,
                Name = (string)row.name,
                ParentCode = parentCode,
                Type = int.Parse((string)row.type),
                OldCode = (string?)row.old_code
            });
            count++;
        }
        else if (!_context.AddressDivisions.Any(a => a.Code == parentCode))
        {
            // Bỏ qua hoặc log lỗi nếu parent không tồn tại
            Console.WriteLine($"Parent {parentCode} chưa có, bỏ qua {code}");
        }
    }

    await _context.SaveChangesAsync();

    return Ok(new { message = $"Đã import {count} dòng thành công." });
}
    }
}
