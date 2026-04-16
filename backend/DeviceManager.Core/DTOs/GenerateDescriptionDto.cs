using System.ComponentModel.DataAnnotations;

namespace DeviceManager.Core.DTOs;

public class GenerateDescriptionDto
{
    [Required] public string Name            { get; set; } = string.Empty;
    [Required] public string Manufacturer    { get; set; } = string.Empty;
    [Required] public string Type            { get; set; } = string.Empty;
    [Required] public string OperatingSystem { get; set; } = string.Empty;
    [Required] public string Processor       { get; set; } = string.Empty;
    [Required] public int    RamAmount       { get; set; }
}

public class GeneratedDescriptionDto
{
    public string Description { get; set; } = string.Empty;
}