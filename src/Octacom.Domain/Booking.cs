using Octacom.Domain.Base;
using Octacom.Domain.ValueObjects;

namespace Octacom.Domain;

public class Booking : Entity
{
    private const string ConfirmationCodeCharacters = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
    private static readonly Random Random = new();


    public Guid ConferenceId { get; private set; }
    public string AttendeeName { get; private set; }
    public Email AttendeeEmail { get;private  set; }
    public BookingStatus Status { get; private set; }
    public string ConfirmationCode { get; private set; }

    public DateTime BookedAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }

    public Booking(Guid id, Guid conferenceId, string attendeeName, Email attendeeEmail) 
        : base(id)
    {
        GuardAgainstInvalidAttendeeName(attendeeName);
        GuardAgainstInvalidConferenceId(conferenceId);
        GuardAgainstInvalidEAttendeeEmail(attendeeEmail);


        ConferenceId = conferenceId;
        AttendeeName = attendeeName;
        AttendeeEmail = attendeeEmail;
        Status = BookingStatus.Confirmed;
        BookedAt = DateTime.UtcNow;
        CancelledAt = null;
        ConfirmationCode = GenerateConfirmationCode();
    }

    #region Guards

    private static void GuardAgainstInvalidEAttendeeEmail(Email attendeeEmail)
    {
        if (attendeeEmail is null)
            throw new ArgumentException("Attendee email cannot be null.", nameof(attendeeEmail));
    }

    private static void GuardAgainstInvalidConferenceId(Guid conferenceId)
    {
        if (conferenceId == Guid.Empty)
            throw new ArgumentException("ConferenceId cannot be empty.", nameof(conferenceId));
    }

    private static void GuardAgainstInvalidAttendeeName(string attendeeName)
    {
        if (string.IsNullOrWhiteSpace(attendeeName))
            throw new ArgumentException("Attendee name cannot be empty.", nameof(attendeeName));
    }

    #endregion
    private static string GenerateConfirmationCode()
    {
        var chars = new char[5];
        for (int i = 0; i < chars.Length; i++)
        {
            chars[i] = ConfirmationCodeCharacters[Random.Next(ConfirmationCodeCharacters.Length)];
        }

        return $"OCT-{new string(chars)}";
    }

    public void Cancel()
    {
        if (Status == BookingStatus.Cancelled)
            throw new InvalidOperationException("*already cancelled*");

        Status = BookingStatus.Cancelled;
        CancelledAt = DateTime.UtcNow;
    }
    public void MarkAsWaitlisted()
    {
        Status = BookingStatus.Waitlisted;
    }

    public void Confirm()
    {
        Status = BookingStatus.Confirmed;
    }

}