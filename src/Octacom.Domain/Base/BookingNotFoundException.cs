namespace Octacom.Domain.Base;

public class BookingNotFoundException : Exception
{
    public BookingNotFoundException(Guid bookingId)
        : base($"Booking with id '{bookingId}' was not found.") { }
}