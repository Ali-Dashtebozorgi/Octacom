using FluentAssertions;
using Moq;
using Octacom.Application.DTOs;
using Octacom.Application.Interfaces;
using Octacom.Domain;
using Octacom.Domain.Base;
using Octacom.Domain.Repositories;
using Octacom.Domain.ValueObjects;

namespace Octacom.UnitTests.Application;
public class ConferenceServiceTests
{
    private readonly Mock<IConferenceRepository> _conferenceRepositoryMock;
    private readonly ConferenceService _sut;

    public ConferenceServiceTests()
    {
        _conferenceRepositoryMock = new Mock<IConferenceRepository>();
        _sut = new ConferenceService(_conferenceRepositoryMock.Object);
    }

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
    // CreateConference
    // -------------------------------------------------------

    [Fact]
    public async Task CreateConference_WhenValid_ShouldReturnConferenceResponse()
    {
        
        var request = new CreateConferenceRequest
        {
            Name = "Test Conference",
            TotalCapacity = 100
        };

        var result = await _sut.CreateConference(request);
        
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.Name.Should().Be(request.Name);
        result.TotalCapacity.Should().Be(request.TotalCapacity);
        result.BookedSeats.Should().Be(0);
        result.AvailableSeats.Should().Be(request.TotalCapacity);
    }

    [Fact]
    public async Task CreateConference_WhenValid_ShouldCallRepositoryAdd()
    {
        
        var request = new CreateConferenceRequest
        {
            Name = "Test Conference",
            TotalCapacity = 100
        };

        await _sut.CreateConference(request);
        
        _conferenceRepositoryMock.Verify(r => r.Add(It.IsAny<Conference>()), Times.Once);
    }

    [Fact]
    public async Task CreateConference_WhenNameIsEmpty_ShouldThrowArgumentException()
    {
        
        var request = new CreateConferenceRequest
        {
            Name = "",
            TotalCapacity = 100
        };

        Func<Task> act = async () => await _sut.CreateConference(request);
        
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task CreateConference_WhenCapacityIsZero_ShouldThrowArgumentException()
    {
        
        var request = new CreateConferenceRequest
        {
            Name = "Test Conference",
            TotalCapacity = 0
        };

        
        Func<Task> act = async () => await _sut.CreateConference(request);

        
        await act.Should().ThrowAsync<ArgumentException>();
    }

    // -------------------------------------------------------
    // GetConference
    // -------------------------------------------------------

    [Fact]
    public async Task GetConference_WhenConferenceExists_ShouldReturnConferenceResponse()
    {
        var conference = CreateConference();
        _conferenceRepositoryMock
            .Setup(r => r.GetById(conference.Id))
            .ReturnsAsync(conference);

        var result = await _sut.GetConference(conference.Id);

        result.Should().NotBeNull();
        result.Id.Should().Be(conference.Id);
        result.Name.Should().Be(conference.Name);
        result.TotalCapacity.Should().Be(conference.TotalCapacity);
        result.AvailableSeats.Should().Be(conference.AvailableSeats);
    }

    [Fact]
    public async Task GetConference_WhenConferenceDoesNotExist_ShouldThrowConferenceNotFoundException()
    {
        _conferenceRepositoryMock
            .Setup(r => r.GetById(It.IsAny<Guid>()))
            .ReturnsAsync((Conference?)null);

        Func<Task> act = async () => await _sut.GetConference(Guid.NewGuid());

        await act.Should().ThrowAsync<ConferenceNotFoundException>();
    }

    // -------------------------------------------------------
    // BookSeat
    // -------------------------------------------------------

    [Fact]
    public async Task BookSeat_WhenValid_ShouldReturnBookingResponse()
    {
        var conference = CreateConference();
        _conferenceRepositoryMock
            .Setup(r => r.GetByIdWithBookings(conference.Id))
            .ReturnsAsync(conference);

        var request = new BookSeatRequest
        {
            AttendeeName = "Ali Dasht",
            AttendeeEmail = "ali@test.com"
        };

        var result = await _sut.BookSeat(conference.Id, request);

        result.Should().NotBeNull();
        result.AttendeeName.Should().Be(request.AttendeeName);
        result.AttendeeEmail.Should().Be(request.AttendeeEmail);
        result.ConferenceId.Should().Be(conference.Id);
    }

    [Fact]
    public async Task BookSeat_WhenValid_ShouldCallRepositoryUpdate()
    {
        var conference = CreateConference();
        _conferenceRepositoryMock
            .Setup(r => r.GetByIdWithBookings(conference.Id))
            .ReturnsAsync(conference);

        var request = new BookSeatRequest
        {
            AttendeeName = "Ali Dasht",
            AttendeeEmail = "ali@test.com"
        };

        await _sut.BookSeat(conference.Id, request);

        _conferenceRepositoryMock.Verify(r => r.Update(conference), Times.Once);
    }

    [Fact]
    public async Task BookSeat_WhenConferenceDoesNotExist_ShouldThrowConferenceNotFoundException()
    {
        _conferenceRepositoryMock
            .Setup(r => r.GetByIdWithBookings(It.IsAny<Guid>()))
            .ReturnsAsync((Conference?)null);

        var request = new BookSeatRequest
        {
            AttendeeName = "Ali Dasht",
            AttendeeEmail = "ali@test.com"
        };

        Func<Task> act = async () => await _sut.BookSeat(Guid.NewGuid(), request);

        await act.Should().ThrowAsync<ConferenceNotFoundException>();
    }

    [Fact]
    public async Task BookSeat_WhenConferenceIsFull_ShouldReturnBookingWithWaitlistedStatus()
    {
        var conference = CreateConference(totalCapacity: 1);
        var existingBooking = CreateBooking(conference.Id, "existing@test.com");
        conference.BookSeat(existingBooking);

        _conferenceRepositoryMock
            .Setup(r => r.GetByIdWithBookings(conference.Id))
            .ReturnsAsync(conference);

        var request = new BookSeatRequest
        {
            AttendeeName = "Ali Dasht",
            AttendeeEmail = "ali@test.com"
        };

        var result = await _sut.BookSeat(conference.Id, request);

        result.Status.Should().Be("Waitlisted");
    }

    [Fact]
    public async Task BookSeat_WhenDuplicateEmail_ShouldThrowDuplicateBookingException()
    {
        var conference = CreateConference();
        var existingBooking = CreateBooking(conference.Id, "ali@test.com");
        conference.BookSeat(existingBooking);

        _conferenceRepositoryMock
            .Setup(r => r.GetByIdWithBookings(conference.Id))
            .ReturnsAsync(conference);

        var request = new BookSeatRequest
        {
            AttendeeName = "Ali Dasht",
            AttendeeEmail = "ali@test.com"
        };

        Func<Task> act = async () => await _sut.BookSeat(conference.Id, request);

        await act.Should().ThrowAsync<DuplicateBookingException>();
    }

    // -------------------------------------------------------
    // CancelSeat
    // -------------------------------------------------------

    [Fact]
    public async Task CancelSeat_WhenValid_ShouldCallRepositoryUpdate()
    {
        var conference = CreateConference();
        var booking = CreateBooking(conference.Id);
        conference.BookSeat(booking);

        _conferenceRepositoryMock
            .Setup(r => r.GetByIdWithBookings(conference.Id))
            .ReturnsAsync(conference);

        await _sut.CancelSeat(conference.Id, booking.Id);

        _conferenceRepositoryMock.Verify(r => r.Update(conference), Times.Once);
    }

    [Fact]
    public async Task CancelSeat_WhenConferenceDoesNotExist_ShouldThrowConferenceNotFoundException()
    {
        
        _conferenceRepositoryMock
            .Setup(r => r.GetByIdWithBookings(It.IsAny<Guid>()))
            .ReturnsAsync((Conference?)null);

        Func<Task> act = async () => await _sut.CancelSeat(Guid.NewGuid(), Guid.NewGuid());
        
        await act.Should().ThrowAsync<ConferenceNotFoundException>();
    }

    [Fact]
    public async Task CancelSeat_WhenBookingDoesNotExist_ShouldThrowBookingNotFoundException()
    {
        var conference = CreateConference();
        _conferenceRepositoryMock
            .Setup(r => r.GetByIdWithBookings(conference.Id))
            .ReturnsAsync(conference);
        
        Func<Task> act = async () => await _sut.CancelSeat(conference.Id, Guid.NewGuid());

        await act.Should().ThrowAsync<BookingNotFoundException>();
    }

    // -------------------------------------------------------
    // GetBooking
    // -------------------------------------------------------

    [Fact]
    public async Task GetBooking_WhenExists_ShouldReturnBookingResponse()
    {
        
        var conference = CreateConference();
        var booking = CreateBooking(conference.Id);
        conference.BookSeat(booking);
        _conferenceRepositoryMock
            .Setup(r => r.GetByIdWithBookings(conference.Id))
            .ReturnsAsync(conference);
        
        var result = await _sut.GetBooking(conference.Id, booking.Id);

        
        result.Should().NotBeNull();
        result.Id.Should().Be(booking.Id);
        result.AttendeeEmail.Should().Be(booking.AttendeeEmail.Value);
    }

    [Fact]
    public async Task GetBooking_WhenBookingDoesNotExist_ShouldThrowBookingNotFoundException()
    {
        
        var conference = CreateConference();
        _conferenceRepositoryMock
            .Setup(r => r.GetByIdWithBookings(conference.Id))
            .ReturnsAsync(conference);

        Func<Task> act = async () => await _sut.GetBooking(conference.Id, Guid.NewGuid());
        
        await act.Should().ThrowAsync<BookingNotFoundException>();
    }

    // -------------------------------------------------------
    // GetBookingsByConference
    // -------------------------------------------------------

    [Fact]
    public async Task GetBookingsByConference_WhenConferenceExists_ShouldReturnAllBookings()
    {
        var conference = CreateConference();
        conference.BookSeat(CreateBooking(conference.Id, "ali@test.com"));
        conference.BookSeat(CreateBooking(conference.Id, "john@test.com"));
        conference.BookSeat(CreateBooking(conference.Id, "jane@test.com"));
        _conferenceRepositoryMock
            .Setup(r => r.GetByIdWithBookings(conference.Id))
            .ReturnsAsync(conference);

        var result = await _sut.GetBookingsByConference(conference.Id);
        
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetBookingsByConference_WhenConferenceDoesNotExist_ShouldThrowConferenceNotFoundException()
    {
        _conferenceRepositoryMock
            .Setup(r => r.GetByIdWithBookings(It.IsAny<Guid>()))
            .ReturnsAsync((Conference?)null);
        
        Func<Task> act = async () => await _sut.GetBookingsByConference(Guid.NewGuid());
        
        await act.Should().ThrowAsync<ConferenceNotFoundException>();
    }

    // -------------------------------------------------------
    // GetAll
    // -------------------------------------------------------

    [Fact]
    public async Task GetAll_WhenConferencesExist_ShouldReturnAllConferences()
    {
        
        var conference1 = CreateConference(totalCapacity: 50);
        var conference2 = CreateConference(totalCapacity: 100);

        _conferenceRepositoryMock
            .Setup(r => r.GetAll())
            .ReturnsAsync(new List<Conference> { conference1, conference2 });

        var result = await _sut.GetAll();

        result.Should().HaveCount(2);
        result.Should().Contain(c => c.Id == conference1.Id);
        result.Should().Contain(c => c.Id == conference2.Id);
    }

    [Fact]
    public async Task GetAll_WhenNoConferencesExist_ShouldReturnEmptyList()
    {
        _conferenceRepositoryMock
            .Setup(r => r.GetAll())
            .ReturnsAsync(new List<Conference>());

        var result = await _sut.GetAll();

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAll_ShouldReturnCorrectlyMappedConferenceResponse()
    {
        var conference = CreateConference(totalCapacity: 30);

        _conferenceRepositoryMock
            .Setup(r => r.GetAll())
            .ReturnsAsync(new List<Conference> { conference });

        var result = await _sut.GetAll();

        var response = result.Single();
        response.Id.Should().Be(conference.Id);
        response.Name.Should().Be(conference.Name);
        response.TotalCapacity.Should().Be(conference.TotalCapacity);
        response.BookedSeats.Should().Be(conference.BookedSeats);
        response.AvailableSeats.Should().Be(conference.AvailableSeats);
    }

}