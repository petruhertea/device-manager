using DeviceManager.Core.DTOs;

namespace DeviceManager.Core.Interfaces;

public interface IDeviceService
{
    Task<IEnumerable<DeviceDto>> GetAllAsync();
    Task<DeviceDto?> GetByIdAsync(int id);
    Task<DeviceDto> CreateAsync(CreateDeviceDto dto);
    Task<DeviceDto?> UpdateAsync(int id, UpdateDeviceDto dto);
    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// Sets or clears the AssignedUserId for a device without touching any
    /// other fields. Used by the employee-accessible assignment endpoint.
    /// Returns null if the device does not exist.
    /// </summary>
    Task<DeviceDto?> AssignAsync(int deviceId, int? userId);
}