using Microsoft.AspNetCore.Mvc;
using RentMaster.Accounts.Services;
using RentMaster.Core.Controllers;
using RentMaster.Core.Middleware;

namespace RentMaster.Management.Consumer.Controller;

[ApiController]
[Attributes.LandLordScope]
[Route("landlords/api/consumers")]
public class ConsumerManagementController : BaseController<Accounts.Models.Consumer>
{
    private readonly ConsumerService _consumerService;

    public ConsumerManagementController(ConsumerService consumerService)
        : base(consumerService)
    {
        _consumerService = consumerService;
    }
}