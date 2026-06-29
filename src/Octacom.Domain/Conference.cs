using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Octacom.Domain.Base;

namespace Octacom.Domain
{
    public class Conference : AggregateRoot
    {
        public string Name { get; private set; }
        public int TotalCapacity { get; private set; }
        public int BookedSeats { get; private set; }
        public byte[] RowVersion { get; private set; } = [];
        public IReadOnlyCollection<Booking> Bookings => _bookings.AsReadOnly();
        private readonly List<Booking> _bookings = new();

        public int AvailableSeats => TotalCapacity - BookedSeats;

        public Conference(Guid id, string name, int totalCapacity) : base(id)
        {
            GuardAgainstInvalidConferenceName(name);
            GuardAgainstInvalidCapacity(totalCapacity);

            Name = name;
            TotalCapacity = totalCapacity;
            BookedSeats = 0;
        }

        #region Guards

        private static void GuardAgainstInvalidCapacity(int totalCapacity)
        {
            if (totalCapacity <= 0)
                throw new ArgumentException("Total capacity must be greater than zero.", nameof(totalCapacity));
        }

        private static void GuardAgainstInvalidConferenceName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Conference name cannot be empty.", nameof(name));
        }
        #endregion


        public void BookSeat(Booking booking)
        {
            if (booking is null)
                throw new ArgumentNullException(nameof(booking));

            if (BookedSeats >= TotalCapacity)
                throw new ConferenceFullException();

            bool isDuplicate = _bookings.Any(b =>
                b.AttendeeEmail.Value == booking.AttendeeEmail.Value &&
                b.Status == BookingStatus.Confirmed);

            if (isDuplicate)
                throw new DuplicateBookingException(booking.AttendeeEmail.Value);

            _bookings.Add(booking);
            BookedSeats++;
        }

        public void CancelSeat(Guid bookingId)
        {
            var booking = _bookings.FirstOrDefault(b => b.Id == bookingId)
                          ?? throw new BookingNotFoundException(bookingId);

            booking.Cancel();
            BookedSeats--;
        }
    }
}
