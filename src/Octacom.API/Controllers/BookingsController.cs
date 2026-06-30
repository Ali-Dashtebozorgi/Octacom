using Microsoft.AspNetCore.Mvc;
using Octacom.API.Common;
using Octacom.Application.DTOs;
using Octacom.Application.Interfaces;

namespace Octacom.API.Controllers;

[ApiController]
[Route("api/conferences/{conferenceId:guid}/bookings")]
public class BookingsController : ControllerBase
{
    private readonly IConferenceService _conferenceService;

    public BookingsController(IConferenceService conferenceService)
    {
        _conferenceService = conferenceService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<BookingResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<BookingResponse>>> BookSeat(Guid conferenceId, [FromBody] BookSeatRequest request)
    {
        var result = await _conferenceService.BookSeat(conferenceId, request);
        return CreatedAtAction(nameof(GetBooking), new { conferenceId, id = result.Id }, ApiResponse<BookingResponse>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<BookingResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<BookingResponse>>> GetBooking(Guid conferenceId, Guid id)
    {
        var result = await _conferenceService.GetBooking(conferenceId, id);
        return Ok(ApiResponse<BookingResponse>.Ok(result));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<object>>> CancelSeat(Guid conferenceId, Guid id)
    {
        await _conferenceService.CancelSeat(conferenceId, id);
        return Ok(ApiResponse<object>.Ok(null!));
    }
}