using DeviceManager.Core.DTOs;
using DeviceManager.Core.Interfaces;
using DeviceManager.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace DeviceManager.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DevicesController(IDeviceRepository repository) : ControllerBase
{
    // GET api/devices
    [HttpGet]
    public async Task<ActionResult<IEnumerable<DeviceDto>>> GetAll()
    {
        var devices = await repository.GetAllAsync();
        return Ok(devices.Select(ToDto));
    }

    // GET api/devices/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<DeviceDto>> GetById(int id)
    {
        var device = await repository.GetByIdAsync(id);
        return device is null ? NotFound() : Ok(ToDto(device));
    }

    // POST api/devices
    [HttpPost]
    public async Task<ActionResult<DeviceDto>> Create([FromBody] CreateDeviceDto dto)
    {
        var device = new Device
        {
            Name = dto.Name,
            Manufacturer = dto.Manufacturer,
            Type = dto.Type,
            OperatingSystem = dto.OperatingSystem,
            OsVersion = dto.OsVersion,
            Processor = dto.Processor,
            RamAmount = dto.RamAmount,
            Description = dto.Description,
            AssignedUserId = dto.AssignedUserId
        };

        var created = await repository.CreateAsync(device);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, ToDto(created));
    }

    // PUT api/devices/5
    [HttpPut("{id:int}")]
    public async Task<ActionResult<DeviceDto>> Update(int id, [FromBody] UpdateDeviceDto dto)
    {
        var updated = await repository.UpdateAsync(id, new Device
        {
            Name = dto.Name,
            Manufacturer = dto.Manufacturer,
            Type = dto.Type,
            OperatingSystem = dto.OperatingSystem,
            OsVersion = dto.OsVersion,
            Processor = dto.Processor,
            RamAmount = dto.RamAmount,
            Description = dto.Description,
            AssignedUserId = dto.AssignedUserId
        });

        return updated is null ? NotFound() : Ok(ToDto(updated));
    }

    // DELETE api/devices/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await repository.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }

    // Private mapper — you could use AutoMapper later, this is fine for now
    private static DeviceDto ToDto(Device d) => new()
    {
        Id = d.Id,
        Name = d.Name,
        Manufacturer = d.Manufacturer,
        Type = d.Type,
        OperatingSystem = d.OperatingSystem,
        OsVersion = d.OsVersion,
        Processor = d.Processor,
        RamAmount = d.RamAmount,
        Description = d.Description,
        AssignedUserName = d.AssignedUser?.Name
    };
}