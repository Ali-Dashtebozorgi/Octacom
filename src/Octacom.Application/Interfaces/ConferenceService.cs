using Octacom.Application.DTOs;
using Octacom.Domain;
using Octacom.Domain.Base;
using Octacom.Domain.Repositories;
using Octacom.Domain.ValueObjects;

namespace Octacom.Application.Interfaces;

public class ConferenceService : IConferenceService
{
    private readonly IConferenceRepository _conferenceRepository;

    public ConferenceService(IConferenceRepository conferenceRepository)
    {
        _conferenceRepository = conferenceRepository;
    }

    public async Task<ConferenceResponse> CreateConference(CreateConferenceRequest request)
    {
        var conference = new Conference(
            id: Guid.NewGuid(),
            name: request.Name,
            totalCapacity: request.TotalCapacity
        );

        await _conferenceRepository.Add(conference);

        return MapToConferenceResponse(conference);
    }

    public async Task<ConferenceResponse> GetConference(Guid conferenceId)
    {
        var conference = await _conferenceRepository.GetById(conferenceId)
                         ?? throw new ConferenceNotFoundException(conferenceId);

        return MapToConferenceResponse(conference);
    }

    public async Task<BookingResponse> BookSeat(Guid conferenceId, BookSeatRequest request)
    {
        var conference = await _conferenceRepository.GetByIdWithBookings(conferenceId)
                         ?? throw new ConferenceNotFoundException(conferenceId);

        var booking = new Booking(
            id: Guid.NewGuid(),
            conferenceId: conferenceId,
            attendeeName: request.AttendeeName,
            attendeeEmail: new Email(request.AttendeeEmail)
        );

        conference.BookSeat(booking);

        await _conferenceRepository.Update(conference);

        return MapToBookingResponse(booking);
    }

    public async Task CancelSeat(Guid conferenceId, Guid bookingId)
    {
        var conference = await _conferenceRepository.GetByIdWithBookings(conferenceId)
                         ?? throw new ConferenceNotFoundException(conferenceId);

        conference.CancelSeat(bookingId);

        await _conferenceRepository.Update(conference);
    }

    public async Task<BookingResponse> GetBooking(Guid conferenceId, Guid bookingId)
    {
        var conference = await _conferenceRepository.GetByIdWithBookings(conferenceId)
                         ?? throw new ConferenceNotFoundException(conferenceId);

        var booking = conference.Bookings.FirstOrDefault(b => b.Id == bookingId)
                      ?? throw new BookingNotFoundException(bookingId);

        return MapToBookingResponse(booking);
    }

    public async Task<IEnumerable<BookingResponse>> GetBookingsByConference(Guid conferenceId)
    {
        var conference = await _conferenceRepository.GetByIdWithBookings(conferenceId)
                         ?? throw new ConferenceNotFoundException(conferenceId);

        return conference.Bookings.Select(MapToBookingResponse);
    }

    public async Task<List<ConferenceResponse>> GetAll()
    {
        var conferences = await _conferenceRepository.GetAll();
        return conferences
            .Select(MapToConferenceResponse)
            .ToList();

    }

    private static ConferenceResponse MapToConferenceResponse(Conference conference) => new()
    {
        Id = conference.Id,
        Name = conference.Name,
        TotalCapacity = conference.TotalCapacity,
        BookedSeats = conference.BookedSeats,
        AvailableSeats = conference.AvailableSeats
    };

    private static BookingResponse MapToBookingResponse(Booking booking) => new()
    {
        Id = booking.Id,
        ConferenceId = booking.ConferenceId,
        AttendeeName = booking.AttendeeName,
        AttendeeEmail = booking.AttendeeEmail.Value,
        Status = booking.Status.ToString(),
        BookedAt = booking.BookedAt,
        CancelledAt = booking.CancelledAt
    };
}