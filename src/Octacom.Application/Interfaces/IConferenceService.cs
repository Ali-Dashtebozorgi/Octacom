using Octacom.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Octacom.Application.Interfaces;
public interface IConferenceService
{
    Task<ConferenceResponse> CreateConference(CreateConferenceRequest request);
    Task<ConferenceResponse> GetConference(Guid conferenceId);
    Task<BookingResponse> BookSeat(Guid conferenceId, BookSeatRequest request);
    Task CancelSeat(Guid conferenceId, Guid bookingId);
    Task<BookingResponse> GetBooking(Guid conferenceId, Guid bookingId);
    Task<PagedResult<BookingResponse>> GetBookingsByConference(Guid conferenceId, int page, int pageSize);
    Task<PagedResult<ConferenceResponse>> GetAll(int page, int pageSize);
}

