namespace Octacom.Domain.Base;

public class ConferenceNotFoundException : Exception
{
    public ConferenceNotFoundException(Guid conferenceId)
        : base($"Conference with id '{conferenceId}' was not found.") { }
}