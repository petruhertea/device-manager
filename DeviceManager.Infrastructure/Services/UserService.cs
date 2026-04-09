using DeviceManager.Core.DTOs;
using DeviceManager.Core.Interfaces;
using DeviceManager.Core.Models;

namespace DeviceManager.Infrastructure.Services;

public class UserService(IUserRepository repository) : IUserService
{
    public async Task<IEnumerable<UserDto>> GetAllAsync()
    {
        var users = await repository.GetAllAsync();
        return users.Select(ToDto);
    }

    public async Task<UserDto?> GetByIdAsync(int id)
    {
        var user = await repository.GetByIdAsync(id);
        return user is null ? null : ToDto(user);
    }

    private static UserDto ToDto(User u) => new()
    {
        Id       = u.Id,
        Name     = u.Name,
        Role     = u.Role,
        Location = u.Location,
        AssignedDeviceNames = u.AssignedDevices.Select(d => d.Name)
    };
}