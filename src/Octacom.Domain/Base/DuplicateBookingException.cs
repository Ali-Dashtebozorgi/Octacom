namespace Octacom.Domain.Base;

public class DuplicateBookingException : Exception
{
    public DuplicateBookingException(string email)
        : base($"A booking with email '{email}' already exists.") { }
}