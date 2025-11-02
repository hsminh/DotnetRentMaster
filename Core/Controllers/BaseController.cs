using Microsoft.AspNetCore.Mvc;
using RentMaster.Core.Services;
using RentMaster.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RentMaster.Core.Controllers;

[ApiController]
[Route("[controller]/api")]
public abstract class BaseController<T> : ControllerBase where T : class
{
    protected readonly BaseService<T> Service;

    protected BaseController(BaseService<T> service)
    {
        Service = service;
    }

    [HttpGet]
    public virtual async Task<IActionResult> GetAll()
    {
        var entities = await Service.GetAllAsync();
        return Ok(entities);
    }

    [HttpGet("{id}")]
    public virtual async Task<IActionResult> GetByUid(Guid id)
    {
        var entity = await Service.GetByUidAsync(id);
        if (entity == null)
            return NotFound();
        
        return Ok(entity);
    }

    [HttpPost] 
    public virtual async Task<IActionResult> Create([FromBody] T model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var created = await Service.CreateAsync(model);
            return CreatedAtAction(nameof(GetByUid), new { id = GetEntityId(created) }, created);
        }
        catch (ValidationException ex)
        {
            return BadRequest(ex.Errors);
        }
   
    }

    [HttpPut("{id:guid}")]
    public virtual async Task<IActionResult> Update(Guid id, [FromBody] T model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var existingEntity = await Service.GetByUidAsync(id);
        if (existingEntity == null)
            return NotFound();

        var properties = typeof(T).GetProperties();
        foreach (var prop in properties)
        {
            if (prop.Name == "Uid") continue;
            var value = prop.GetValue(model);
            prop.SetValue(existingEntity, value);
        }

        try
        {
            await Service.UpdateAsync(existingEntity);
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



    [HttpDelete("{id}")]
    public virtual async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await Service.DeleteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    protected virtual Guid GetEntityId(T entity)
    {
        var uidProperty = entity.GetType().GetProperty("Uid");
        if (uidProperty != null)
        {
            return (Guid)uidProperty.GetValue(entity)!;
        }
        throw new NotImplementedException("Entity must have a Uid property");
    }
}
