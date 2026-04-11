using DeviceManager.Core.Interfaces;
using DeviceManager.Core.Models;
using DeviceManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DeviceManager.Infrastructure.Repositories;

public class UserRepository(AppDbContext context) : IUserRepository
{
    public async Task<IEnumerable<ApplicationUser>> GetAllAsync()
        => await context.Users
            .Include(u => u.AssignedDevices)
            .ToListAsync();

    public async Task<ApplicationUser?> GetByIdAsync(int id)
        => await context.Users
            .Include(u => u.AssignedDevices)
            .FirstOrDefaultAsync(u => u.Id == id);
}