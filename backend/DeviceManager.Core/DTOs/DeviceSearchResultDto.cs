using System.Text.Json.Serialization;

namespace DeviceManager.Core.DTOs;

public class DeviceSearchResultDto
{
    public int    Id               { get; set; }
    public string Name             { get; set; } = string.Empty;
    public string Manufacturer     { get; set; } = string.Empty;
    public string Type             { get; set; } = string.Empty;
    public string OperatingSystem  { get; set; } = string.Empty;
    public string OsVersion        { get; set; } = string.Empty;
    public string Processor        { get; set; } = string.Empty;
    public int    RamAmount        { get; set; }
    public string Description      { get; set; } = string.Empty;
    public string? AssignedUserName { get; set; }

    /// <summary>Relevance score — higher is a stronger match. Not exposed to the client.</summary>
    [JsonIgnore]
    public int Score { get; set; }
}