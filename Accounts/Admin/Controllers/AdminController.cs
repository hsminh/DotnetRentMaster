using Microsoft.AspNetCore.Mvc;
using RentMaster.Accounts.Admin.Services;
using RentMaster.Accounts.LandLords.Models;
using RentMaster.Accounts.LandLords.Service;
using RentMaster.Core.Controllers;
using RentMaster.Core.Exceptions;
using RentMaster.Core.Middleware;
using RentMaster.Core.Utils;

namespace RentMaster.Accounts.Admin.Controllers;

[ApiController]
[Attributes.AdminScope]
[Route("[controller]/api/landlords")]
public class AdminController : BaseController<LandLord>
{
    private readonly LandLordService _landlordService;

    public AdminController(AdminService service, LandLordService landLordService)
        : base(landLordService)
    {
        _landlordService = landLordService;
    }

    [HttpGet("{id}")]
    public override async Task<IActionResult> GetByUid(Guid id)
    {
        var result = await base.GetByUid(id);
        if (result is OkObjectResult okResult && okResult.Value != null)
        {
            MarkPassword.MaskPasswordIfExist(okResult.Value);
            return Ok(okResult.Value);
        }
        return result;
    }

    [HttpGet]
    public override async Task<IActionResult> GetAll()
    {
        var result = await base.GetAll();
        if (result is OkObjectResult okResult && okResult.Value != null)
        {
            MarkPassword.MaskPasswordIfExist(okResult.Value);
            return Ok(okResult.Value);
        }
        return result;
    }

    [HttpPut("{id:guid}")]
    public override async Task<IActionResult> Update(Guid id, [FromBody] LandLord model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var existingEntity = await _landlordService.GetByUidAsync(id);
        if (existingEntity == null)
            return NotFound();

        var properties = typeof(LandLord).GetProperties();
        foreach (var prop in properties)
        {
            if (prop.Name == "Uid") continue;
            
            var value = prop.GetValue(model);
            
            // Bỏ qua password đã bị mask
            if (prop.Name == "Password" && value?.ToString() == "*************")
                continue;
                
            prop.SetValue(existingEntity, value);
        }

        try
        {
            await _landlordService.UpdateAsync(existingEntity);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (ValidationException ex)
        {
            return BadRequest(ex.Errors);
        }
        catch (Exception ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }
}