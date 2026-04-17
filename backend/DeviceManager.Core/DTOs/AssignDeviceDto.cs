namespace DeviceManager.Core.DTOs;

/// <summary>
/// Payload for PATCH /api/v1/devices/{id}/assignment.
/// Send <c>userId</c> to assign, <c>null</c> to unassign.
/// </summary>
public class AssignDeviceDto
{
    public int? UserId { get; set; }
}