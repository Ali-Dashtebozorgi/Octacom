namespace Octacom.Domain.Base;

public class ConferenceFullException : Exception
{
    public ConferenceFullException()
        : base("Conference has no available seats.") { }
}