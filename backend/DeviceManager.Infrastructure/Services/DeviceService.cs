using DeviceManager.Core.DTOs;
using DeviceManager.Core.Interfaces;
using DeviceManager.Core.Models;

namespace DeviceManager.Infrastructure.Services;

public class DeviceService(
    IDeviceRepository repository,
    IDescriptionGeneratorService descriptionGenerator)
    : IDeviceService
{
    public async Task<IEnumerable<DeviceDto>> GetAllAsync()
        => (await repository.GetAllAsync()).Select(ToDto);

    public async Task<DeviceDto?> GetByIdAsync(int id)
    {
        var device = await repository.GetByIdAsync(id);
        return device is null ? null : ToDto(device);
    }

    public async Task<DeviceDto> CreateAsync(CreateDeviceDto dto)
    {
        var device = new Device
        {
            Name            = dto.Name,
            Manufacturer    = dto.Manufacturer,
            Type            = dto.Type,
            OperatingSystem = dto.OperatingSystem,
            OsVersion       = dto.OsVersion,
            Processor       = dto.Processor,
            RamAmount       = dto.RamAmount,
            Description     = dto.Description,
            AssignedUserId  = dto.AssignedUserId
        };

        if (string.IsNullOrWhiteSpace(device.Description))
            device.Description = await descriptionGenerator.GenerateAsync(
                device.Name, device.Manufacturer, device.Type,
                device.OperatingSystem, device.Processor, device.RamAmount)
                ?? string.Empty;

        return ToDto(await repository.CreateAsync(device));
    }

    public async Task<DeviceDto?> UpdateAsync(int id, UpdateDeviceDto dto)
    {
        var device = new Device
        {
            Name            = dto.Name,
            Manufacturer    = dto.Manufacturer,
            Type            = dto.Type,
            OperatingSystem = dto.OperatingSystem,
            OsVersion       = dto.OsVersion,
            Processor       = dto.Processor,
            RamAmount       = dto.RamAmount,
            Description     = dto.Description,
            AssignedUserId  = dto.AssignedUserId
        };

        if (string.IsNullOrWhiteSpace(device.Description))
            device.Description = await descriptionGenerator.GenerateAsync(
                device.Name, device.Manufacturer, device.Type,
                device.OperatingSystem, device.Processor, device.RamAmount)
                ?? string.Empty;

        var updated = await repository.UpdateAsync(id, device);
        return updated is null ? null : ToDto(updated);
    }

    public async Task<DeviceDto?> AssignAsync(int deviceId, int? userId)
    {
        // Load the current device so we can patch only AssignedUserId
        var existing = await repository.GetByIdAsync(deviceId);
        if (existing is null) return null;

        var patched = new Device
        {
            Name            = existing.Name,
            Manufacturer    = existing.Manufacturer,
            Type            = existing.Type,
            OperatingSystem = existing.OperatingSystem,
            OsVersion       = existing.OsVersion,
            Processor       = existing.Processor,
            RamAmount       = existing.RamAmount,
            Description     = existing.Description,
            AssignedUserId  = userId      // the only field that changes
        };

        var updated = await repository.UpdateAsync(deviceId, patched);
        return updated is null ? null : ToDto(updated);
    }

    public async Task<bool> DeleteAsync(int id)
        => await repository.DeleteAsync(id);

    private static DeviceDto ToDto(Device d) => new()
    {
        Id               = d.Id,
        Name             = d.Name,
        Manufacturer     = d.Manufacturer,
        Type             = d.Type,
        OperatingSystem  = d.OperatingSystem,
        OsVersion        = d.OsVersion,
        Processor        = d.Processor,
        RamAmount        = d.RamAmount,
        Description      = d.Description,
        AssignedUserName = d.AssignedUser?.FullName
    };
}