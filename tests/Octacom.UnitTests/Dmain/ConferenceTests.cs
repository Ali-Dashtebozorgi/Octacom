using FluentAssertions;
using Octacom.Domain;
using Octacom.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Octacom.Domain.Base;

namespace Octacom.UnitTests.Dmain;

public class ConferenceTests
{
    private Conference CreateConference(int totalCapacity = 10)
    {
        return new Conference(
            id: Guid.NewGuid(),
            name: "Test Conference",
            totalCapacity: totalCapacity
        );
    }

    private Booking CreateBooking(Guid conferenceId, string email = "ali@test.com", string name = "Ali Dasht")
    {
        return new Booking(
            id: Guid.NewGuid(),
            conferenceId: conferenceId,
            attendeeName: name,
            attendeeEmail: new Email(email)
        );
    }

    // -------------------------------------------------------
    // Constructor Guards
    // -------------------------------------------------------

    [Fact]
    public void NewConference_WhenNameIsEmpty_ShouldThrowArgumentException()
    {
        Action act = () => new Conference(Guid.NewGuid(), "", 100);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void NewConference_WhenNameIsNull_ShouldThrowArgumentException()
    {
        Action act = () => new Conference(Guid.NewGuid(), null!, 100);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void NewConference_WhenTotalCapacityIsZero_ShouldThrowArgumentException()
    {
        Action act = () => new Conference(Guid.NewGuid(), "Test Conference", 0);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void NewConference_WhenTotalCapacityIsNegative_ShouldThrowArgumentException()
    {
        Action act = () => new Conference(Guid.NewGuid(), "Test Conference", -1);

        act.Should().Throw<ArgumentException>();
    }

    // -------------------------------------------------------
    // Initial State
    // -------------------------------------------------------

    [Fact]
    public void NewConference_ShouldHaveZeroBookedSeats()
    {
        var conference = CreateConference();

        conference.BookedSeats.Should().Be(0);
    }

    [Fact]
    public void NewConference_AvailableSeats_ShouldEqualTotalCapacity()
    {
        var conference = CreateConference(totalCapacity: 10);

        conference.AvailableSeats.Should().Be(10);
    }

    [Fact]
    public void NewConference_ShouldHaveEmptyBookings()
    {
        var conference = CreateConference();

        conference.Bookings.Should().BeEmpty();
    }

    // -------------------------------------------------------
    // BookSeat
    // -------------------------------------------------------

    [Fact]
    public void BookSeat_WhenSeatsAvailable_ShouldIncrementBookedSeats()
    {
        var conference = CreateConference(totalCapacity: 10);
        var booking = CreateBooking(conference.Id);

        conference.BookSeat(booking);

        conference.BookedSeats.Should().Be(1);
    }

    [Fact]
    public void BookSeat_WhenSeatsAvailable_ShouldAddBookingToBookings()
    {
        var conference = CreateConference(totalCapacity: 10);
        var booking = CreateBooking(conference.Id);

        conference.BookSeat(booking);

        conference.Bookings.Should().ContainSingle(b => b.AttendeeEmail.Value == "ali@test.com");
    }

    [Fact]
    public void BookSeat_WhenConferenceIsFull_ShouldWaitlistTheBooking()
    {
        var conference = CreateConference(totalCapacity: 1);
        var firstBooking = CreateBooking(conference.Id, "ali@test.com");
        var secondBooking = CreateBooking(conference.Id, "john@test.com");

        conference.BookSeat(firstBooking);
        conference.BookSeat(secondBooking);

        secondBooking.Status.Should().Be(BookingStatus.Waitlisted);
    }

    [Fact]
    public void BookSeat_WhenConferenceIsFull_ShouldNotIncrementBookedSeats()
    {
        var conference = CreateConference(totalCapacity: 1);
        var firstBooking = CreateBooking(conference.Id, "ali@test.com");
        var secondBooking = CreateBooking(conference.Id, "john@test.com");

        conference.BookSeat(firstBooking);
        conference.BookSeat(secondBooking);

        conference.BookedSeats.Should().Be(1);
    }

    [Fact]
    public void BookSeat_WhenConferenceIsFull_ShouldStillAddBookingToBookingsList()
    {
        var conference = CreateConference(totalCapacity: 1);
        var firstBooking = CreateBooking(conference.Id, "ali@test.com");
        var secondBooking = CreateBooking(conference.Id, "john@test.com");

        conference.BookSeat(firstBooking);
        conference.BookSeat(secondBooking);

        conference.Bookings.Should().HaveCount(2);
    }

    [Fact]
    public void BookSeat_WhenSameEmailBooksTwice_ShouldThrowDuplicateBookingException()
    {
        var conference = CreateConference(totalCapacity: 10);
        var firstBooking = CreateBooking(conference.Id, "ali@test.com");
        var duplicateBooking = CreateBooking(conference.Id, "ali@test.com");
        conference.BookSeat(firstBooking);

        Action act = () => conference.BookSeat(duplicateBooking);

        act.Should().Throw<DuplicateBookingException>();
    }

    [Fact]
    public void BookSeat_WhenSameEmailWithDifferentCase_ShouldThrowDuplicateBookingException()
    {
        var conference = CreateConference(totalCapacity: 10);
        var firstBooking = CreateBooking(conference.Id, "ali@test.com");
        var duplicateBooking = CreateBooking(conference.Id, "ALI@TEST.COM");
        conference.BookSeat(firstBooking);

        Action act = () => conference.BookSeat(duplicateBooking);

        act.Should().Throw<DuplicateBookingException>();
    }

    [Fact]
    public void BookSeat_MultipleValidBookings_ShouldIncrementCorrectly()
    {
        var conference = CreateConference(totalCapacity: 10);

        conference.BookSeat(CreateBooking(conference.Id, "ali@test.com"));
        conference.BookSeat(CreateBooking(conference.Id, "john@test.com"));
        conference.BookSeat(CreateBooking(conference.Id, "jane@test.com"));

        conference.BookedSeats.Should().Be(3);
    }

    [Fact]
    public void BookSeat_WhenNullBooking_ShouldThrowArgumentNullException()
    {
        var conference = CreateConference();

        Action act = () => conference.BookSeat(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    // -------------------------------------------------------
    // CancelSeat
    // -------------------------------------------------------

    [Fact]
    public void CancelSeat_WhenBookingExists_ShouldDecrementBookedSeats()
    {
        var conference = CreateConference(totalCapacity: 10);
        var booking = CreateBooking(conference.Id);
        conference.BookSeat(booking);

        conference.CancelSeat(booking.Id);

        conference.BookedSeats.Should().Be(0);
    }

    [Fact]
    public void CancelSeat_WhenBookingExists_ShouldMarkBookingAsCancelled()
    {
        var conference = CreateConference(totalCapacity: 10);
        var booking = CreateBooking(conference.Id);
        conference.BookSeat(booking);

        conference.CancelSeat(booking.Id);

        booking.Status.Should().Be(BookingStatus.Cancelled);
    }

    [Fact]
    public void CancelSeat_WhenBookingDoesNotExist_ShouldThrowBookingNotFoundException()
    {
        var conference = CreateConference(totalCapacity: 10);

        Action act = () => conference.CancelSeat(Guid.NewGuid());

        act.Should().Throw<BookingNotFoundException>();
    }

    [Fact]
    public void CancelSeat_WhenAlreadyCancelled_ShouldThrowInvalidOperationException()
    {
        var conference = CreateConference(totalCapacity: 10);
        var booking = CreateBooking(conference.Id);
        conference.BookSeat(booking);
        conference.CancelSeat(booking.Id);

        Action act = () => conference.CancelSeat(booking.Id);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void CancelSeat_ShouldAllowRebookingAfterCancellation()
    {
        var conference = CreateConference(totalCapacity: 1);
        var booking = CreateBooking(conference.Id, "ali@test.com");
        conference.BookSeat(booking);
        conference.CancelSeat(booking.Id);

        var newBooking = CreateBooking(conference.Id, "ali@test.com");
        Action act = () => conference.BookSeat(newBooking);

        act.Should().NotThrow();
    }

    // -------------------------------------------------------
    // AvailableSeats
    // -------------------------------------------------------

    [Fact]
    public void AvailableSeats_AfterBookingAndCancelling_ShouldBeCorrect()
    {
        var conference = CreateConference(totalCapacity: 10);
        var booking = CreateBooking(conference.Id);
        conference.BookSeat(booking);
        conference.CancelSeat(booking.Id);

        conference.AvailableSeats.Should().Be(10);
    }
    // -------------------------------------------------------
    // Waitlist Promotion
    // -------------------------------------------------------

    [Fact]
    public void CancelSeat_WhenWaitlistedBookingExists_ShouldPromoteEarliestWaitlistedBookingToConfirmed()
    {
        var conference = CreateConference(totalCapacity: 1);
        var confirmedBooking = CreateBooking(conference.Id, "ali@test.com");
        var waitlistedBooking = CreateBooking(conference.Id, "john@test.com");

        conference.BookSeat(confirmedBooking);
        conference.BookSeat(waitlistedBooking);

        conference.CancelSeat(confirmedBooking.Id);

        waitlistedBooking.Status.Should().Be(BookingStatus.Confirmed);
    }

    [Fact]
    public void CancelSeat_WhenWaitlistedBookingIsPromoted_ShouldIncrementBookedSeats()
    {
        var conference = CreateConference(totalCapacity: 1);
        var confirmedBooking = CreateBooking(conference.Id, "ali@test.com");
        var waitlistedBooking = CreateBooking(conference.Id, "john@test.com");

        conference.BookSeat(confirmedBooking);
        conference.BookSeat(waitlistedBooking);

        conference.CancelSeat(confirmedBooking.Id);

        conference.BookedSeats.Should().Be(1);
    }

    [Fact]
    public void CancelSeat_WhenMultipleWaitlistedBookingsExist_ShouldPromoteEarliestOneOnly()
    {
        var conference = CreateConference(totalCapacity: 1);
        var confirmedBooking = CreateBooking(conference.Id, "ali@test.com");
        var firstWaitlisted = CreateBooking(conference.Id, "john@test.com");
        var secondWaitlisted = CreateBooking(conference.Id, "jane@test.com");

        conference.BookSeat(confirmedBooking);
        conference.BookSeat(firstWaitlisted);
        conference.BookSeat(secondWaitlisted);

        conference.CancelSeat(confirmedBooking.Id);

        firstWaitlisted.Status.Should().Be(BookingStatus.Confirmed);
        secondWaitlisted.Status.Should().Be(BookingStatus.Waitlisted);
    }

    [Fact]
    public void CancelSeat_WhenNoWaitlistedBookingsExist_ShouldNotThrowAndJustDecrementSeats()
    {
        var conference = CreateConference(totalCapacity: 10);
        var booking = CreateBooking(conference.Id, "ali@test.com");
        conference.BookSeat(booking);

        conference.CancelSeat(booking.Id);

        conference.BookedSeats.Should().Be(0);
    }

    [Fact]
    public void CancelSeat_WhenCancellingAWaitlistedBooking_ShouldNotPromoteAnyoneOrChangeBookedSeats()
    {
        var conference = CreateConference(totalCapacity: 1);
        var confirmedBooking = CreateBooking(conference.Id, "ali@test.com");
        var waitlistedBooking = CreateBooking(conference.Id, "john@test.com");

        conference.BookSeat(confirmedBooking);
        conference.BookSeat(waitlistedBooking);

        conference.CancelSeat(waitlistedBooking.Id);

        conference.BookedSeats.Should().Be(1);
        waitlistedBooking.Status.Should().Be(BookingStatus.Cancelled);
    }
}

