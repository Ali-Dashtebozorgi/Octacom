using Microsoft.AspNetCore.Mvc;
using Octacom.API.Common;
using Octacom.Application.DTOs;
using Octacom.Application.Interfaces;

namespace Octacom.API.Controllers;

[ApiController]
[Route("api/conferences")]
public class ConferencesController : ControllerBase
{
    private readonly IConferenceService _conferenceService;

    public ConferencesController(IConferenceService conferenceService)
    {
        _conferenceService = conferenceService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ConferenceResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ConferenceResponse>>> CreateConference([FromBody] CreateConferenceRequest request)
    {
        var result = await _conferenceService.CreateConference(request);
        return CreatedAtAction(nameof(GetConference), new { id = result.Id }, ApiResponse<ConferenceResponse>.Ok(result));
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ConferenceResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<ConferenceResponse>>>> GetConferences([FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var conferences = await _conferenceService.GetAll(page, pageSize);
        return Ok(ApiResponse<PagedResult<ConferenceResponse>>.Ok(conferences));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ConferenceResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ConferenceResponse>>> GetConference(Guid id)
    {
        var result = await _conferenceService.GetConference(id);
        return Ok(ApiResponse<ConferenceResponse>.Ok(result));
    }

    [HttpGet("{id:guid}/bookings")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<BookingResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PagedResult<BookingResponse>>>> GetBookings(Guid id, [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _conferenceService.GetBookingsByConference(id, page, pageSize);
        return Ok(ApiResponse<PagedResult<BookingResponse>>.Ok(result));
    }
}