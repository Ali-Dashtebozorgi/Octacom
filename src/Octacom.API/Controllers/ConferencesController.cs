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
    [ProducesResponseType(typeof(ApiResponse<List<ConferenceResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<ConferenceResponse>>>> GetConferences()
    {
        var conferences = await _conferenceService.GetAll();
        return Ok(ApiResponse<List<ConferenceResponse>>.Ok(conferences));
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
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<BookingResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<IEnumerable<BookingResponse>>>> GetBookings(Guid id)
    {
        var result = await _conferenceService.GetBookingsByConference(id);
        return Ok(ApiResponse<IEnumerable<BookingResponse>>.Ok(result));
    }
}