using System.Net;
using System.Net.Http.Json;
using DeviceManager.Core.DTOs;
using FluentAssertions;

namespace DeviceManager.IntegrationTests;

[Collection("DevicesCollection")]
public class DevicesControllerTests : IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly DeviceManagerWebAppFactory _factory;

    private const string DeviceEndpoint = "/api/v1/devices";

    public DevicesControllerTests(DeviceManagerWebAppFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    // Runs before each test — equivalent to @BeforeEach in JUnit
    public Task InitializeAsync()
    {
        _factory.ResetDatabase();
        return Task.CompletedTask;
    }

    // Runs after each test — nothing to clean up here, reset handles it
    public Task DisposeAsync() => Task.CompletedTask;

    // -------------------------------------------------------------------------
    // GET api/devices
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetAll_ReturnsOk_WithSeededDevices()
    {
        var response = await _client.GetAsync(DeviceEndpoint);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var devices = await response.Content.ReadFromJsonAsync<List<DeviceDto>>();
        devices.Should().NotBeNull();
        devices!.Count.Should().Be(2);
    }

    // -------------------------------------------------------------------------
    // GET api/devices/{id}
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetById_ExistingId_ReturnsCorrectDevice()
    {
        var response = await _client.GetAsync(DeviceEndpoint + "/1");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var device = await response.Content.ReadFromJsonAsync<DeviceDto>();
        device.Should().NotBeNull();
        device!.Name.Should().Be("iPhone 15 Pro");
        device.Manufacturer.Should().Be("Apple");
        device.AssignedUserName.Should().Be("Alice Johnson");
    }

    [Fact]
    public async Task GetById_NonExistentId_ReturnsNotFound()
    {
        var response = await _client.GetAsync(DeviceEndpoint + "/999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // -------------------------------------------------------------------------
    // POST api/devices
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Create_ValidDevice_ReturnsCreated_WithLocation()
    {
        var newDevice = new CreateDeviceDto
        {
            Name            = "Pixel 8",
            Manufacturer    = "Google",
            Type            = "phone",
            OperatingSystem = "Android",
            OsVersion       = "14.0",
            Processor       = "Tensor G3",
            RamAmount       = 8,
            Description     = "Google flagship"
        };

        var response = await _client.PostAsJsonAsync(DeviceEndpoint, newDevice);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();

        var created = await response.Content.ReadFromJsonAsync<DeviceDto>();
        created.Should().NotBeNull();
        created!.Name.Should().Be("Pixel 8");
        created.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Create_InvalidJson_ReturnsBadRequest()
    {
        var content = new StringContent(
            "{ this is not valid json }",
            System.Text.Encoding.UTF8,
            "application/json");

        var response = await _client.PostAsync(DeviceEndpoint, content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Create_InvalidOsVersion_ReturnsBadRequest_WithValidationError()
    {
        var newDevice = new CreateDeviceDto
        {
            Name            = "Test Phone",
            Manufacturer    = "Test",
            Type            = "phone",
            OperatingSystem = "Android",
            OsVersion       = "16",       // missing the .0 — should fail
            Processor       = "Snapdragon",
            RamAmount       = 8,
            Description     = "Test"
        };

        var response = await _client.PostAsJsonAsync(DeviceEndpoint, newDevice);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await response.Content.ReadFromJsonAsync<ValidationErrorResponse>();
        body!.Errors.Should().ContainKey("OsVersion");
    }

    [Fact]
    public async Task Create_InvalidType_ReturnsBadRequest_WithValidationError()
    {
        var newDevice = new CreateDeviceDto
        {
            Name            = "Test",
            Manufacturer    = "Test",
            Type            = "laptop",   // not phone or tablet
            OperatingSystem = "Windows",
            OsVersion       = "11.0",
            Processor       = "Intel",
            RamAmount       = 16,
            Description     = "Test"
        };

        var response = await _client.PostAsJsonAsync(DeviceEndpoint, newDevice);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await response.Content.ReadFromJsonAsync<ValidationErrorResponse>();
        body!.Errors.Should().ContainKey("Type");
    }

    [Fact]
    public async Task Create_MissingRequiredFields_ReturnsBadRequest()
    {
        // Completely empty body
        var response = await _client.PostAsJsonAsync(DeviceEndpoint, new { });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // -------------------------------------------------------------------------
    // PUT api/devices/{id}
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Update_ExistingDevice_ReturnsOk_WithUpdatedValues()
    {
        var update = new UpdateDeviceDto
        {
            Name            = "iPhone 15 Pro Max",
            Manufacturer    = "Apple",
            Type            = "phone",
            OperatingSystem = "iOS",
            OsVersion       = "17.5",
            Processor       = "A17 Pro",
            RamAmount       = 8,
            Description     = "Updated model"
        };

        var response = await _client.PutAsJsonAsync(DeviceEndpoint + "/1", update);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var updated = await response.Content.ReadFromJsonAsync<DeviceDto>();
        updated!.Name.Should().Be("iPhone 15 Pro Max");
        updated.OsVersion.Should().Be("17.5");
    }

    [Fact]
    public async Task Update_NonExistentId_ReturnsNotFound()
    {
        var update = new UpdateDeviceDto
        {
            Name = "Ghost Device", Manufacturer = "Nobody",
            Type = "phone", OperatingSystem = "iOS",
            OsVersion = "1.0", Processor = "X1",
            RamAmount = 4, Description = "Does not exist"
        };

        var response = await _client.PutAsJsonAsync(DeviceEndpoint + "/999", update);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // -------------------------------------------------------------------------
    // DELETE api/devices/{id}
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Delete_ExistingDevice_ReturnsNoContent()
    {
        var response = await _client.DeleteAsync(DeviceEndpoint + "/2");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync(DeviceEndpoint + "/2");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_NonExistentId_ReturnsNotFound()
    {
        var response = await _client.DeleteAsync(DeviceEndpoint + "/999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}