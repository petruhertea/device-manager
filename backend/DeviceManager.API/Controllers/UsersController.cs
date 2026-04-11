using DeviceManager.Core.DTOs;
using DeviceManager.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeviceManager.Controllers;

[ApiController]
[Route("api/v1/users")]
[Authorize]
public class UsersController(IUserService service) : ControllerBase
{
    [HttpGet]
    [EndpointSummary("Retrieve all users.")]
    [EndpointDescription("Returns a collection of users from the company.")]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAll()
        => Ok(await service.GetAllAsync());

    [HttpGet("{id:int}")]
    [EndpointSummary("Retrieve user.")]
    [EndpointDescription("Returns one user by their id.")]
    public async Task<ActionResult<UserDto>> GetById(int id)
    {
        var user = await service.GetByIdAsync(id);
        return user is null ? NotFound() : Ok(user);
    }
    
    // TODO Phase 3: add POST /register and POST /login endpoints here
    // TODO Phase 3: add GET /me endpoint to return the currently authenticated user
}