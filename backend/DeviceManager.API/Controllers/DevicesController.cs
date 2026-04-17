using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using DeviceManager.Core.DTOs;
using DeviceManager.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeviceManager.Controllers;

[ApiController]
[Route("api/v1/devices")]
[Authorize]                        // all endpoints require authentication at minimum
public class DevicesController(
    IDeviceService service,
    IDescriptionGeneratorService descriptionGenerator,
    IDeviceSearchService searchService)
    : ControllerBase
{
    // ── Read (any authenticated user) ─────────────────────────────────────────

    [HttpGet]
    [Authorize(Policy = "AnyUser")]
    [EndpointSummary("Retrieve all devices.")]
    public async Task<ActionResult<IEnumerable<DeviceDto>>> GetAll()
        => Ok(await service.GetAllAsync());

    [HttpGet("search")]
    [Authorize(Policy = "AnyUser")]
    [EndpointSummary("Search devices.")]
    public async Task<ActionResult<IEnumerable<DeviceSearchResultDto>>> Search([FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest(new { message = "Query parameter 'q' is required." });

        return Ok(await searchService.SearchAsync(q));
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = "AnyUser")]
    [EndpointSummary("Retrieve device.")]
    public async Task<ActionResult<DeviceDto>> GetById(int id)
    {
        var device = await service.GetByIdAsync(id);
        return device is null ? NotFound() : Ok(device);
    }

    // ── Write (admin only) ────────────────────────────────────────────────────

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    [EndpointSummary("Create device.")]
    public async Task<ActionResult<DeviceDto>> Create([FromBody] CreateDeviceDto dto)
    {
        var created = await service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "AdminOnly")]
    [EndpointSummary("Update device.")]
    public async Task<ActionResult<DeviceDto>> Update(int id, [FromBody] UpdateDeviceDto dto)
    {
        var updated = await service.UpdateAsync(id, dto);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "AdminOnly")]
    [EndpointSummary("Delete device.")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await service.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }

    [HttpPost("generate-description")]
    [Authorize(Policy = "AdminOnly")]
    [EndpointSummary("Generate device description.")]
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

    // ── Assignment (any authenticated user, with ownership enforcement) ────────

    [HttpPatch("{id:int}/assignment")]
    [Authorize(Policy = "AnyUser")]
    [EndpointSummary("Assign or unassign a device.")]
    [EndpointDescription(
        "Employees may only assign a free device to themselves, or unassign a device " +
        "that is currently assigned to them. Admins may assign to any user or clear any assignment.")]
    public async Task<ActionResult<DeviceDto>> UpdateAssignment(
        int id, [FromBody] AssignDeviceDto dto)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId is null)
            return Unauthorized();

        var isAdmin = User.HasClaim("role", "Admin");

        // Load current state so we can enforce the ownership rules
        var device = await service.GetByIdAsync(id);
        if (device is null) return NotFound();

        if (!isAdmin)
        {
            var assigning   = dto.UserId is not null;
            var unassigning = dto.UserId is null;

            if (assigning)
            {
                // Employee can only assign to themselves
                if (dto.UserId != currentUserId)
                    return Forbid();

                // Device must be free
                if (device.AssignedUserName is not null)
                    return Conflict(new { message = "Device is already assigned to another user." });
            }

            if (unassigning)
            {
                // Employee can only unassign their own device
                // We check by resolving who owns it — the name comparison is safe because
                // AssignedUserName is populated from the DB join, not from user input
                var ownerId = await GetUserIdByName(device.AssignedUserName);
                if (ownerId != currentUserId)
                    return Forbid();
            }
        }

        var updated = await service.AssignAsync(id, dto.UserId);
        return updated is null ? NotFound() : Ok(updated);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private int? GetCurrentUserId()
    {
        var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
               ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(sub, out var id) ? id : null;
    }

    /// <summary>
    /// Resolves a user ID from their FullName via the claims on the current principal.
    /// This is only used for the employee unassign ownership check.
    /// Because we store the name in the JWT we can compare without a DB lookup.
    /// </summary>
    private Task<int?> GetUserIdByName(string? fullName)
    {
        // The assigned user's name comes from the DB; the current user's name is in
        // the ClaimTypes.Name claim (set to FullName in AuthService.GenerateJwt).
        if (fullName is null) return Task.FromResult<int?>(null);

        var currentName = User.FindFirst("name")!.Value;
        if (currentName != fullName) return Task.FromResult<int?>(null);

        // Names match → the current user is the owner
        return Task.FromResult(GetCurrentUserId());
    }
}