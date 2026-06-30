using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Octacom.API.Common;
using Octacom.Application.DTOs;

namespace Octacom.IntegrationTests;

public class BookingsControllerTests : IntegrationTestBase
{
    public BookingsControllerTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    private async Task<Guid> CreateConferenceAsync(string name = "Tech Summit 2026", int capacity = 10)
    {
        var response = await Client.PostAsJsonAsync("/api/conferences",
            new CreateConferenceRequest { Name = name, TotalCapacity = capacity });

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<ConferenceResponse>>();
        return body!.Data!.Id;
    }

    private async Task<BookingResponse> BookSeatAsync(Guid conferenceId, string email = "ali@test.com", string name = "Ali Dasht")
    {
        var response = await Client.PostAsJsonAsync($"/api/conferences/{conferenceId}/bookings",
            new BookSeatRequest { AttendeeName = name, AttendeeEmail = email });

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<BookingResponse>>();
        return body!.Data!;
    }

    // -------------------------------------------------------
    // POST /api/conferences/{id}/bookings
    // -------------------------------------------------------

    [Fact]
    public async Task BookSeat_WhenValid_ShouldReturn201AndIncrementBookedSeats()
    {
        
        var conferenceId = await CreateConferenceAsync(capacity: 10);
        var request = new BookSeatRequest { AttendeeName = "Ali Dasht", AttendeeEmail = "ali@test.com" };

        
        var response = await Client.PostAsJsonAsync($"/api/conferences/{conferenceId}/bookings", request);

        
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<BookingResponse>>();
        body!.Success.Should().BeTrue();
        body.Data!.AttendeeName.Should().Be("Ali Dasht");
        body.Data.AttendeeEmail.Should().Be("ali@test.com");
        body.Data.Status.Should().Be("Confirmed");

        // Verify BookedSeats incremented via the conference endpoint
        var conferenceResponse = await Client.GetAsync($"/api/conferences/{conferenceId}");
        var conferenceBody = await conferenceResponse.Content.ReadFromJsonAsync<ApiResponse<ConferenceResponse>>();
        conferenceBody!.Data!.BookedSeats.Should().Be(1);
        conferenceBody.Data.AvailableSeats.Should().Be(9);
    }

    [Fact]
    public async Task BookSeat_WhenConferenceIsFull_ShouldReturn409()
    {
        
        var conferenceId = await CreateConferenceAsync(capacity: 1);
        await BookSeatAsync(conferenceId, "first@test.com");

        var request = new BookSeatRequest { AttendeeName = "John Doe", AttendeeEmail = "john@test.com" };

        
        var response = await Client.PostAsJsonAsync($"/api/conferences/{conferenceId}/bookings", request);

        
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        body!.Success.Should().BeFalse();
    }

    [Fact]
    public async Task BookSeat_WhenDuplicateEmail_ShouldReturn409()
    {
        
        var conferenceId = await CreateConferenceAsync(capacity: 10);
        await BookSeatAsync(conferenceId, "ali@test.com");

        var request = new BookSeatRequest { AttendeeName = "Ali Dasht", AttendeeEmail = "ali@test.com" };

        
        var response = await Client.PostAsJsonAsync($"/api/conferences/{conferenceId}/bookings", request);

        
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task BookSeat_WhenEmailFormatIsInvalid_ShouldReturn400()
    {
        
        var conferenceId = await CreateConferenceAsync(capacity: 10);
        var request = new BookSeatRequest { AttendeeName = "Ali Dasht", AttendeeEmail = "not-an-email" };

        
        var response = await Client.PostAsJsonAsync($"/api/conferences/{conferenceId}/bookings", request);

        
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task BookSeat_WhenConferenceDoesNotExist_ShouldReturn404()
    {
        
        var request = new BookSeatRequest { AttendeeName = "Ali Dasht", AttendeeEmail = "ali@test.com" };

        
        var response = await Client.PostAsJsonAsync($"/api/conferences/{Guid.NewGuid()}/bookings", request);

        
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // -------------------------------------------------------
    // GET /api/conferences/{id}/bookings/{id}
    // -------------------------------------------------------

    [Fact]
    public async Task GetBooking_WhenExists_ShouldReturn200WithCorrectBooking()
    {
        
        var conferenceId = await CreateConferenceAsync(capacity: 10);
        var booking = await BookSeatAsync(conferenceId, "ali@test.com", "Ali Dasht");

        
        var response = await Client.GetAsync($"/api/conferences/{conferenceId}/bookings/{booking.Id}");

        
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<BookingResponse>>();
        body!.Data!.Id.Should().Be(booking.Id);
        body.Data.AttendeeName.Should().Be("Ali Dasht");
        body.Data.AttendeeEmail.Should().Be("ali@test.com");
    }

    [Fact]
    public async Task GetBooking_WhenDoesNotExist_ShouldReturn404()
    {
        
        var conferenceId = await CreateConferenceAsync(capacity: 10);

        
        var response = await Client.GetAsync($"/api/conferences/{conferenceId}/bookings/{Guid.NewGuid()}");

        
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // -------------------------------------------------------
    // DELETE /api/conferences/{id}/bookings/{id}
    // -------------------------------------------------------

    [Fact]
    public async Task CancelSeat_WhenValid_ShouldReturn200AndDecrementBookedSeats()
    {
        
        var conferenceId = await CreateConferenceAsync(capacity: 10);
        var booking = await BookSeatAsync(conferenceId, "ali@test.com");

        
        var response = await Client.DeleteAsync($"/api/conferences/{conferenceId}/bookings/{booking.Id}");

        
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify BookedSeats decremented
        var conferenceResponse = await Client.GetAsync($"/api/conferences/{conferenceId}");
        var conferenceBody = await conferenceResponse.Content.ReadFromJsonAsync<ApiResponse<ConferenceResponse>>();
        conferenceBody!.Data!.BookedSeats.Should().Be(0);
        conferenceBody.Data.AvailableSeats.Should().Be(10);

        // Verify booking status changed to Cancelled
        var bookingResponse = await Client.GetAsync($"/api/conferences/{conferenceId}/bookings/{booking.Id}");
        var bookingBody = await bookingResponse.Content.ReadFromJsonAsync<ApiResponse<BookingResponse>>();
        bookingBody!.Data!.Status.Should().Be("Cancelled");
        bookingBody.Data.CancelledAt.Should().NotBeNull();
    }

    [Fact]
    public async Task CancelSeat_WhenAlreadyCancelled_ShouldReturn400()
    {
        
        var conferenceId = await CreateConferenceAsync(capacity: 10);
        var booking = await BookSeatAsync(conferenceId, "ali@test.com");
        await Client.DeleteAsync($"/api/conferences/{conferenceId}/bookings/{booking.Id}");

        
        var response = await Client.DeleteAsync($"/api/conferences/{conferenceId}/bookings/{booking.Id}");

        
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CancelSeat_WhenBookingDoesNotExist_ShouldReturn404()
    {
        
        var conferenceId = await CreateConferenceAsync(capacity: 10);

        
        var response = await Client.DeleteAsync($"/api/conferences/{conferenceId}/bookings/{Guid.NewGuid()}");

        
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}