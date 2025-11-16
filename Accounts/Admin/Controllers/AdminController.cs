using Microsoft.AspNetCore.Mvc;
using RentMaster.Accounts.Admin.Services;
using RentMaster.Accounts.LandLords.Models;
using RentMaster.Accounts.LandLords.Service;
using RentMaster.Core.Controllers;
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
}