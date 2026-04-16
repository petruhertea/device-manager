using DeviceManager.Core.Interfaces;
using DeviceManager.Core.Models;
using DeviceManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DeviceManager.Infrastructure.Repositories;

public class DeviceRepository(AppDbContext context) : IDeviceRepository
{
    public async Task<IEnumerable<Device>> GetAllAsync()
        => await context.Devices
            .Include(d => d.AssignedUser) // like @EntityGraph / JOIN FETCH in JPA
            .ToListAsync();

    public async Task<Device?> GetByIdAsync(int id)
        => await context.Devices
            .Include(d => d.AssignedUser)
            .FirstOrDefaultAsync(d => d.Id == id);

    public async Task<Device> CreateAsync(Device device)
    {
        context.Devices.Add(device);
        await context.SaveChangesAsync();
        return device;
    }

    public async Task<Device?> UpdateAsync(int id, Device updated)
    {
        var device = await context.Devices.FindAsync(id);
        if (device is null) return null;

        device.Name = updated.Name;
        device.Manufacturer = updated.Manufacturer;
        device.Type = updated.Type;
        device.OperatingSystem = updated.OperatingSystem;
        device.OsVersion = updated.OsVersion;
        device.Processor = updated.Processor;
        device.RamAmount = updated.RamAmount;
        device.Description = updated.Description;
        device.AssignedUserId = updated.AssignedUserId;

        await context.SaveChangesAsync();
        return device;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var device = await context.Devices.FindAsync(id);
        if (device is null) return false;

        context.Devices.Remove(device);
        await context.SaveChangesAsync();
        return true;
    }
}