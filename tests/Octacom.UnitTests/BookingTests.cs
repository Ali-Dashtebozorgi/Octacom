using FluentAssertions;
using Octacom.Domain;
using Octacom.Domain.Base;
using Octacom.Domain.ValueObjects;

namespace Octacom.UnitTests;

public class BookingTests
{
    private Booking CreateBooking()
    {
        return new Booking(
            id:Guid.NewGuid(),
            conferenceId : Guid.NewGuid(),
            attendeeName : "Ali Dasht",
            attendeeEmail : new Email("ali@gmail.com")
        );
    }

    [Fact]
    public void NewBooking_ShouldHaveConfirmedStatus()
    {
        var booking = CreateBooking();
        booking.Status.Should().Be(BookingStatus.Confirmed);
    }

    [Fact]
    public void NewBooking_ShouldHaveBookedAtSetToUtcNow()
    {
        var before = DateTime.UtcNow;
        var booking = CreateBooking();

        booking.BookedAt.Should().BeOnOrAfter(before);
        booking.BookedAt.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public void NewBooking_CanceledAtShouldBeNull()
    {
        var booking = CreateBooking();

        booking.CancelledAt.Should().BeNull();
    }

    [Fact]
    public void Cancel_WhenConfirmed_ShouldSetCanceledAtToUtcNow()
    {
        var booking = CreateBooking();
        var before = DateTime.UtcNow;
        
        booking.Cancel();

        booking.CancelledAt.Should().NotBeNull();
        booking.CancelledAt.Should().BeOnOrAfter(before);
    }

    [Fact]
    public void Cancel_WhenAlreadyCancelled_ShouldThrowInvalidOperationException()
    {
        var booking = CreateBooking();
        
        booking.Cancel();
        Action act =()=> booking.Cancel();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*already cancelled*");
    }

    [Fact]
    public void NewBooking_WhenAttendeeNameIsEmpty_ShouldThrowArgumentException()
    {
        Action act = () => new Booking(Guid.NewGuid(), Guid.NewGuid(), "", new Email("ali@gmail.com"));

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void NewBooking_WhenAttendeeNameIsNull_ShouldThrowArgumentException()
    {
        Action act = () => new Booking(Guid.NewGuid(), Guid.NewGuid(), null!, new Email("ali@test.com"));

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void NewBooking_WhenConferenceIdIsEmpty_ShouldThrowArgumentException()
    {
        Action act = () => new Booking(Guid.NewGuid(), Guid.Empty, "Ali Dasht", new Email("ali@test.com"));

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void NewBooking_WhenAttendeeEmailIsEmpty_ShouldThrowArgumentException()
    {
        Action act = () => new Booking(Guid.NewGuid(), Guid.NewGuid(), "Ali Dasht", null);

        act.Should().Throw<ArgumentException>();
    }
}