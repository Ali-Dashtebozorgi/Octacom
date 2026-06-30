using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Octacom.API.Common;
using Octacom.Application.DTOs;

namespace Octacom.IntegrationTests;

public class ConferencesControllerTests : IntegrationTestBase
{
    public ConferencesControllerTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    // -------------------------------------------------------
    // POST /api/conferences
    // -------------------------------------------------------

    [Fact]
    public async Task CreateConference_WhenValid_ShouldReturn201WithLocationHeader()
    {
        
        var request = new CreateConferenceRequest
        {
            Name = "Tech Summit 20233",
            TotalCapacity = 50
        };

        
        var response = await Client.PostAsJsonAsync("/api/conferences", request);

        
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<ConferenceResponse>>();
        body!.Success.Should().BeTrue();
        body.Data!.Name.Should().Be(request.Name);
        body.Data.TotalCapacity.Should().Be(request.TotalCapacity);
        body.Data.AvailableSeats.Should().Be(request.TotalCapacity);
        body.Data.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateConference_WhenNameIsEmpty_ShouldReturn400()
    {
        
        var request = new CreateConferenceRequest
        {
            Name = "",
            TotalCapacity = 50
        };

        
        var response = await Client.PostAsJsonAsync("/api/conferences", request);

        
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        body!.Success.Should().BeFalse();
        body.Error.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task CreateConference_WhenCapacityIsZero_ShouldReturn400()
    {
        
        var request = new CreateConferenceRequest
        {
            Name = "Tech Summit 2026",
            TotalCapacity = 0
        };

        
        var response = await Client.PostAsJsonAsync("/api/conferences", request);

        
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // -------------------------------------------------------
    // GET /api/conferences
    // -------------------------------------------------------

    [Fact]
    public async Task GetConferences_WhenConferencesExist_ShouldReturnAllConferences()
    {
        
        await Client.PostAsJsonAsync("/api/conferences", new CreateConferenceRequest { Name = "Conference A", TotalCapacity = 10 });
        await Client.PostAsJsonAsync("/api/conferences", new CreateConferenceRequest { Name = "Conference B", TotalCapacity = 20 });

        
        var response = await Client.GetAsync("/api/conferences");

        
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<List<ConferenceResponse>>>();
        body!.Success.Should().BeTrue();
        body.Data.Should().HaveCount(2);
        body.Data.Should().Contain(c => c.Name == "Conference A");
        body.Data.Should().Contain(c => c.Name == "Conference B");
    }

    [Fact]
    public async Task GetConferences_WhenNoneExist_ShouldReturnEmptyList()
    {
        
        var response = await Client.GetAsync("/api/conferences");
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<List<ConferenceResponse>>>();
        body!.Data.Should().BeEmpty();
    }

    // -------------------------------------------------------
    // GET /api/conferences/{id}
    // -------------------------------------------------------

    [Fact]
    public async Task GetConference_WhenExists_ShouldReturn200WithCorrectData()
    {
        
        var createResponse = await Client.PostAsJsonAsync("/api/conferences",
            new CreateConferenceRequest { Name = "Tech Summit 2026", TotalCapacity = 50 });
        var created = await createResponse.Content.ReadFromJsonAsync<ApiResponse<ConferenceResponse>>();

        
        var response = await Client.GetAsync($"/api/conferences/{created!.Data!.Id}");

        
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<ConferenceResponse>>();
        body!.Data!.Id.Should().Be(created.Data.Id);
        body.Data.Name.Should().Be("Tech Summit 2026");
        body.Data.TotalCapacity.Should().Be(50);
        body.Data.AvailableSeats.Should().Be(50);
    }

    [Fact]
    public async Task GetConference_WhenDoesNotExist_ShouldReturn404()
    {
        
        var response = await Client.GetAsync($"/api/conferences/{Guid.NewGuid()}");

        
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        body!.Success.Should().BeFalse();
    }

    // -------------------------------------------------------
    // GET /api/conferences/{id}/bookings
    // -------------------------------------------------------

    [Fact]
    public async Task GetBookings_WhenBookingsExist_ShouldReturnAllBookingsForConference()
    {
        
        var createResponse = await Client.PostAsJsonAsync("/api/conferences",
            new CreateConferenceRequest { Name = "Tech Summit 2026", TotalCapacity = 50 });
        var conference = await createResponse.Content.ReadFromJsonAsync<ApiResponse<ConferenceResponse>>();
        var conferenceId = conference!.Data!.Id;

        await Client.PostAsJsonAsync($"/api/conferences/{conferenceId}/bookings",
            new BookSeatRequest { AttendeeName = "Ali Dasht", AttendeeEmail = "ali@test.com" });
        await Client.PostAsJsonAsync($"/api/conferences/{conferenceId}/bookings",
            new BookSeatRequest { AttendeeName = "John Doe", AttendeeEmail = "john@test.com" });

        
        var response = await Client.GetAsync($"/api/conferences/{conferenceId}/bookings");

        
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<List<BookingResponse>>>();
        body!.Data.Should().HaveCount(2);
        body.Data.Should().Contain(b => b.AttendeeEmail == "ali@test.com");
        body.Data.Should().Contain(b => b.AttendeeEmail == "john@test.com");
    }

    [Fact]
    public async Task GetBookings_WhenConferenceDoesNotExist_ShouldReturn404()
    {
        
        var response = await Client.GetAsync($"/api/conferences/{Guid.NewGuid()}/bookings");

        
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetBookings_WhenNoBookingsExist_ShouldReturnEmptyList()
    {
        
        var createResponse = await Client.PostAsJsonAsync("/api/conferences",
            new CreateConferenceRequest { Name = "Tech Summit 2026", TotalCapacity = 50 });
        var conference = await createResponse.Content.ReadFromJsonAsync<ApiResponse<ConferenceResponse>>();

        
        var response = await Client.GetAsync($"/api/conferences/{conference!.Data!.Id}/bookings");

        
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<List<BookingResponse>>>();
        body!.Data.Should().BeEmpty();
    }
}