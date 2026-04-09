using DeviceManager.Core.DTOs;
using DeviceManager.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DeviceManager.API.Controllers;

[ApiController]
[Route("api/v1/users")]
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
}