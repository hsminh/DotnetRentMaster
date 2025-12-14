using Microsoft.AspNetCore.Mvc;
using RentMaster.Core.Controllers;
using RentMaster.Core.Exceptions;
using RentMaster.Core.Middleware;
using RentMaster.Management.ConsumerFavorite.Models.DTOs;
using RentMaster.Management.ConsumerFavorite.Services;

namespace RentMaster.Management.ConsumerFavorite.Controllers.ConsumerController;

[ApiController]
[Attributes.UserScope]
[Route("consumer/api/favorite")]
public class ConsumerFavoriteController : BaseController<Models.ConsumerFavorite>
{
    private readonly ConsumerFavoriteService _service;

    public ConsumerFavoriteController(
        ConsumerFavoriteService service) : base(service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }
    
    [HttpPost("create")]
    public async Task<IActionResult> CreateAsync([FromBody] CreateConsumerFavoriteDto model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var consumer = HttpContext.GetCurrentUser<Accounts.Models.Consumer>();
        try
        {
            model.ConsumerId = consumer.Uid;
            var created = await _service.CreateFavoriteAsync(model);

            return CreatedAtAction(nameof(GetByUid), new { id = GetEntityId(created) }, created);
        }
        catch (ValidationException ex)
        {
            return BadRequest(ex.Errors);
        }
    }

    [HttpGet]
    public override async Task<IActionResult> GetAll()
    {
        var consumer = HttpContext.GetCurrentUser<Accounts.Models.Consumer>();
        var result =  await _service.GetAllAsync(consumer);;
        return Ok(result);
    }
}
