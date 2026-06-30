using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Octacom.Domain;
using Octacom.Domain.ValueObjects;
using Octacom.Infrastructure;

namespace Octacom.IntegrationTests;

public class ConferenceConcurrencyTests : IntegrationTestBase
{
    public ConferenceConcurrencyTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    private ConferenceDbContext CreateDbContext()
    {
        var scope = Factory.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<ConferenceDbContext>();
    }

    [Fact]
    public async Task BookSeat_WhenTwoContextsModifySameConferenceConcurrently_SecondSaveShouldThrowDbUpdateConcurrencyException()
    {
        // Arrange — create a conference with only 1 seat, using a separate setup context
        var conferenceId = Guid.NewGuid();
        using (var setupContext = CreateDbContext())
        {
            var conference = new Conference(conferenceId, "Concurrency Test Conference", totalCapacity: 1);
            await setupContext.Conferences.AddAsync(conference);
            await setupContext.SaveChangesAsync();
        }

        // Act — simulate two concurrent requests, each with its own DbContext,
        // both loading the SAME RowVersion of the conference before either writes
        using var context1 = CreateDbContext();
        using var context2 = CreateDbContext();

        var conferenceFromRequest1 = await context1.Conferences.FirstAsync(c => c.Id == conferenceId);
        var conferenceFromRequest2 = await context2.Conferences.FirstAsync(c => c.Id == conferenceId);

        var booking1 = new Booking(Guid.NewGuid(), conferenceId, "Ali Dasht", new Email("ali@test.com"));
        var booking2 = new Booking(Guid.NewGuid(), conferenceId, "John Doe", new Email("john@test.com"));

        conferenceFromRequest1.BookSeat(booking1);
        conferenceFromRequest2.BookSeat(booking2);

        // First request saves successfully — RowVersion in the DB is now updated
        context1.Conferences.Update(conferenceFromRequest1);
        await context1.SaveChangesAsync();

        // Second request tries to save with its now-stale RowVersion
        context2.Conferences.Update(conferenceFromRequest2);
        Func<Task> secondSave = async () => await context2.SaveChangesAsync();

        // Assert
        await secondSave.Should().ThrowAsync<DbUpdateConcurrencyException>();
    }

    [Fact]
    public async Task BookSeat_AfterConcurrencyConflict_DatabaseShouldReflectOnlyTheFirstSuccessfulBooking()
    {
        // Arrange
        var conferenceId = Guid.NewGuid();
        using (var setupContext = CreateDbContext())
        {
            var conference = new Conference(conferenceId, "Concurrency Test Conference", totalCapacity: 1);
            await setupContext.Conferences.AddAsync(conference);
            await setupContext.SaveChangesAsync();
        }

        using var context1 = CreateDbContext();
        using var context2 = CreateDbContext();

        var conferenceFromRequest1 = await context1.Conferences.FirstAsync(c => c.Id == conferenceId);
        var conferenceFromRequest2 = await context2.Conferences.FirstAsync(c => c.Id == conferenceId);

        var booking1 = new Booking(Guid.NewGuid(), conferenceId, "Ali Dasht", new Email("ali@test.com"));
        var booking2 = new Booking(Guid.NewGuid(), conferenceId, "John Doe", new Email("john@test.com"));

        conferenceFromRequest1.BookSeat(booking1);
        conferenceFromRequest2.BookSeat(booking2);

        context1.Conferences.Update(conferenceFromRequest1);
        await context1.SaveChangesAsync();

        context2.Conferences.Update(conferenceFromRequest2);
        try
        {
            await context2.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            // expected — swallow for this test, we only care about final DB state
        }

        // Assert — verify final DB state using a fresh, untouched context
        using var verificationContext = CreateDbContext();
        var finalConference = await verificationContext.Conferences
            .Include(c => c.Bookings)
            .FirstAsync(c => c.Id == conferenceId);

        finalConference.BookedSeats.Should().Be(1);
        finalConference.Bookings.Should().ContainSingle();
        finalConference.Bookings.Single().AttendeeEmail.Value.Should().Be("ali@test.com");
    }
}