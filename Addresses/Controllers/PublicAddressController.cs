using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Mvc;
using RentMaster.Addresses.DTO;
using RentMaster.Addresses.Models;
using RentMaster.Addresses.Services;
namespace RentMaster.Addresses.Controllers;


[ApiController]
[Route("public/address")]
public class PublicAddressController : ControllerBase
{
    private readonly AddressService _service;
  
    private readonly IAddressImportService _addressImportService;

    // ✅ Chỉ 1 constructor, inject tất cả service cần thiết
    public PublicAddressController(AddressService service, IAddressImportService addressImportService)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _addressImportService = addressImportService ?? throw new ArgumentNullException(nameof(addressImportService));
    }

    [HttpGet("province")]
    public async Task<IActionResult> GetProvinces()
    {
        var provinces = await _service.GetProvincesAsync();
        return Ok(provinces);
    }

    [HttpGet("division")]
    public async Task<IActionResult> GetDivisions([FromQuery] string parentUid, [FromQuery] string? type)
    {
        if (string.IsNullOrWhiteSpace(parentUid))
            return BadRequest("parentUid is required");

        var divisions = await _service.GetChildrenByParentUidAsync(parentUid, type);
        return Ok(divisions);
    }
    [HttpPost("import")]
    public async Task<IActionResult> ImportCsv(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "File rỗng" });

        try
        {
            using var stream = file.OpenReadStream();
            using var reader = new StreamReader(stream);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            csv.Context.RegisterClassMap<AddressCsvMap>();
            var records = csv.GetRecords<AddressCsvRecord>().ToList();

            var result = await _addressImportService.ImportFromRecordsAsync(records);

            if (!result.Success)
                return BadRequest(result);

            return Ok(new { message = "Import thành công", imported = result.ImportedCount });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi server", detail = ex.Message });
        }
    }
    [HttpGet("street")]
    public async Task<IActionResult> GetStreets()
    {
        var streets = await _service.GetStreetsAsync();
        return Ok(streets);
    }
    [HttpPost("create")]
    public async Task<IActionResult> CreateAddress([FromBody] CreateAddressDto dto)
    {
        if (dto == null)
            return BadRequest("DTO không được nhận"); // ✅ bây giờ hợp lệ
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest("Tên là bắt buộc"); // ✅ hợp lệ

        var address = new AddressDivision
        {
            Name = dto.Name,
            Type = dto.Type.ToString(),
            Code = Guid.NewGuid().ToString(),
            ParentId = dto.ParentId
        };

        // ✅ await bây giờ hợp lệ vì method async
        var created = await _service.CreateAsync(address);

        return Ok(created);
    }
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var address = await _service.GetByUidAsync(id);

        if (address == null)
            return NotFound(new { message = "Không tìm thấy địa chỉ" });

        return Ok(address); // Trả về JSON entity
    }
    // DELETE 
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            // Tìm địa chỉ theo id
            var address = await _service.GetByUidAsync(id);
            if (address == null)
            {
                return NotFound(new { message = "Địa chỉ không tồn tại" });
            }

            // Xóa
            await _service.DeleteAsync(address);

            return NoContent(); // 204
        }
        
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAddressDto dto)
        {
            var address = await _service.GetByUidAsync(id);
            if (address == null)
                return NotFound(new { message = "Không tìm thấy địa chỉ" });
            address.Name = dto.Name;
            await _service.UpdateAsync(address);
            return Ok(address);
        }

}