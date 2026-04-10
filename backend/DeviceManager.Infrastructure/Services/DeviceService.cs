using DeviceManager.Core.DTOs;
using DeviceManager.Core.Interfaces;
using DeviceManager.Core.Models;

namespace DeviceManager.Infrastructure.Services;

public class DeviceService(IDeviceRepository repository) : IDeviceService
{
    public async Task<IEnumerable<DeviceDto>> GetAllAsync()
    {
        var devices = await repository.GetAllAsync();
        return devices.Select(ToDto);
    }

    public async Task<DeviceDto?> GetByIdAsync(int id)
    {
        var device = await repository.GetByIdAsync(id);
        return device is null ? null : ToDto(device);
    }

    public async Task<DeviceDto> CreateAsync(CreateDeviceDto dto)
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
        return ToDto(created);
    }

    public async Task<DeviceDto?> UpdateAsync(int id, UpdateDeviceDto dto)
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

        var updated = await repository.UpdateAsync(id, device);
        return updated is null ? null : ToDto(updated);
    }

    public async Task<bool> DeleteAsync(int id)
        => await repository.DeleteAsync(id);

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