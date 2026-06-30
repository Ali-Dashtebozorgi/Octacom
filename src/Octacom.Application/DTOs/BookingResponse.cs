namespace Octacom.Application.DTOs;

public class BookingResponse
{
    public Guid Id { get; set; }
    public Guid ConferenceId { get; set; }
    public string AttendeeName { get; set; } = string.Empty;
    public string AttendeeEmail { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime BookedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string ConfirmationCode { get; set; } = string.Empty;

}