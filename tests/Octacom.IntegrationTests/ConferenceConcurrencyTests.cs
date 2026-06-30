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
        Func<Task> secondSave = async () => await context2.SaveChangesAsync();

        await secondSave.Should().ThrowAsync<DbUpdateConcurrencyException>();
    }

    [Fact]
    public async Task BookSeat_AfterConcurrencyConflict_DatabaseShouldReflectOnlyTheFirstSuccessfulBooking()
    {
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
        }

        using var verificationContext = CreateDbContext();
        var finalConference = await verificationContext.Conferences
            .Include(c => c.Bookings)
            .FirstAsync(c => c.Id == conferenceId);

        finalConference.BookedSeats.Should().Be(1);
        finalConference.Bookings.Should().ContainSingle();
        finalConference.Bookings.Single().AttendeeEmail.Value.Should().Be("ali@test.com");
    }
}