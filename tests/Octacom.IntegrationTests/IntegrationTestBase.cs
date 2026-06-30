using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Octacom.Infrastructure;

namespace Octacom.IntegrationTests;

public abstract class IntegrationTestBase : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    protected readonly CustomWebApplicationFactory Factory;
    protected readonly HttpClient Client;

    protected IntegrationTestBase(CustomWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ConferenceDbContext>();

        await context.Database.MigrateAsync();

        // Clean tables before each test for isolation
        context.Bookings.RemoveRange(context.Bookings);
        context.Conferences.RemoveRange(context.Conferences);
        await context.SaveChangesAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;
}