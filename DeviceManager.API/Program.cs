using DeviceManager.Core.Interfaces;
using DeviceManager.Infrastructure.Data;
using DeviceManager.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Register DbContext with SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register repositories
builder.Services.AddScoped<IDeviceRepository, DeviceRepository>();

builder.Services.AddControllers();
builder.Services.AddOpenApi();  // built-in, no extra package needed

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();  // exposes the spec at /openapi/v1.json
    app.MapScalarApiReference();  // UI at /scalar/v1
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();