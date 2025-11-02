using  Microsoft.AspNetCore.Mvc;
using RentMaster.Accounts.Services;
using RentMaster.Core.Controllers;
using RentMaster.Core.Middleware;

namespace RentMaster.Accounts.Admin.Controllers;

[ApiController]
[Attributes.AdminScope]
[Route("[controller]/api/landlords")]
public class AdminController : BaseController<Models.LandLord>
{
    private readonly LandLordService _landlordService;
    public AdminController(AdminService service, LandLordService landLordService) 
        : base(landLordService)
    {
        _landlordService = landLordService;
    }
}