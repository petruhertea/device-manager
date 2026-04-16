using DeviceManager.Core.DTOs;
using DeviceManager.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeviceManager.Controllers;

[ApiController]
[Route("api/v1/devices")]
[Authorize]
public class DevicesController(
    IDeviceService service,
    IDescriptionGeneratorService descriptionGenerator,
    IDeviceSearchService searchService)
    : ControllerBase
{
    [HttpGet]
    [EndpointSummary("Retrieve all devices.")]
    [EndpointDescription("Returns a collection of devices available in the inventory.")]
    public async Task<ActionResult<IEnumerable<DeviceDto>>> GetAll()
        => Ok(await service.GetAllAsync());

    // NOTE: This route must be declared BEFORE {id:int} so the router
    // doesn't try to parse "search" as an integer.
    [HttpGet("search")]
    [EndpointSummary("Search devices.")]
    [EndpointDescription(
        "Returns devices matching the query string, ordered by relevance. " +
        "Searchable fields: Name, Manufacturer, Processor, RAM.")]
    public async Task<ActionResult<IEnumerable<DeviceSearchResultDto>>> Search(
        [FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest(new { message = "Query parameter 'q' is required." });

        var results = await searchService.SearchAsync(q);
        return Ok(results);
    }

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
    [EndpointDescription("Creates a new device. If description is empty, one is auto-generated.")]
    public async Task<ActionResult<DeviceDto>> Create([FromBody] CreateDeviceDto dto)
    {
        var created = await service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [EndpointSummary("Update device.")]
    [EndpointDescription("Updates an existing device. If description is empty, one is auto-generated.")]
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

    [HttpPost("generate-description")]
    [EndpointSummary("Generate device description.")]
    [EndpointDescription("Uses a local LLM to generate a description from device specs.")]
    public async Task<ActionResult<GeneratedDescriptionDto>> GenerateDescription(
        [FromBody] GenerateDescriptionDto dto)
    {
        var description = await descriptionGenerator.GenerateAsync(
            dto.Name, dto.Manufacturer, dto.Type,
            dto.OperatingSystem, dto.Processor, dto.RamAmount);

        if (description is null)
            return StatusCode(503, new { message = "Description generator is currently unavailable." });

        return Ok(new GeneratedDescriptionDto { Description = description });
    }
}