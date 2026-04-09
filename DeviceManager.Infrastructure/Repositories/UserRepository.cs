using DeviceManager.Core.Interfaces;
using DeviceManager.Core.Models;
using DeviceManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DeviceManager.Infrastructure.Repositories;

public class UserRepository(AppDbContext context) : IUserRepository
{
    public async Task<IEnumerable<User>> GetAllAsync()
        => await context.Users
            .Include(u => u.AssignedDevices)
            .ToListAsync();

    public async Task<User?> GetByIdAsync(int id)
        => await context.Users
            .Include(u => u.AssignedDevices)
            .FirstOrDefaultAsync(u => u.Id == id);
}