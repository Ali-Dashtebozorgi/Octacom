namespace Octacom.Application.DTOs;

public class ConferenceResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int TotalCapacity { get; set; }
    public int BookedSeats { get; set; }
    public int AvailableSeats { get; set; }
}