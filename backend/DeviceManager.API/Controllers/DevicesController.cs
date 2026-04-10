using DeviceManager.Core.DTOs;
using DeviceManager.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DeviceManager.Controllers;

[ApiController]
[Route("api/v1/devices")]
public class DevicesController(IDeviceService service) : ControllerBase
{
    [HttpGet]
    [EndpointSummary("Retrieve all devices.")]
    [EndpointDescription("Returns a collection of devices available in the inventory.")]
    public async Task<ActionResult<IEnumerable<DeviceDto>>> GetAll()
        => Ok(await service.GetAllAsync());
    
    [HttpGet("{id:int}")]
    [EndpointSummary("Retrieve device.")]
    [EndpointDescription("Returns a single device by id.")]
    public async Task<ActionResult<DeviceDto>> GetById(int id)
    {
        var device = await service.GetByIdAsync(id);
        return device is null ? NotFound() : Ok(device);
    }

    [HttpPost]
    [EndpointSummary("Create device.")]
    [EndpointDescription("Creates a new device.")]
    public async Task<ActionResult<DeviceDto>> Create([FromBody] CreateDeviceDto dto)
    {
        var created = await service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [EndpointSummary("Update device.")]
    [EndpointDescription("Updates an existing device.")]
    public async Task<ActionResult<DeviceDto>> Update(int id, [FromBody] UpdateDeviceDto dto)
    {
        var updated = await service.UpdateAsync(id, dto);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:int}")]
    [EndpointSummary("Delete device.")]
    [EndpointDescription("Deletes an existing device.")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await service.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}