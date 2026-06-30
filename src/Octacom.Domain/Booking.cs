using Octacom.Domain.Base;
using Octacom.Domain.ValueObjects;

namespace Octacom.Domain;

public class Booking : Entity
{
    public Guid ConferenceId { get; private set; }
    public string AttendeeName { get; private set; }
    public Email AttendeeEmail { get;private  set; }
    public BookingStatus Status { get; private set; }
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