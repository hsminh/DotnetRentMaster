using Microsoft.AspNetCore.Mvc;
using RentMaster.Accounts.Admin.Services;
using RentMaster.Core.Controllers;

namespace RentMaster.Accounts.Auth.Controllers
{
    [ApiController]
    [Route("[controller]/api")]
    public class AdminController : BaseController<Admin.Models.Admin>
    {
        public AdminController(AdminService service) 
            : base(service)
        {
        }
    }
}