using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Octacom.Domain;

namespace Octacom.Infrastructure;

public class DatabaseSeeder
{
    private readonly ConferenceDbContext _context;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(ConferenceDbContext context, ILogger<DatabaseSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        try
        {
            await _context.Database.MigrateAsync();

            if (!await _context.Conferences.AnyAsync())
            {
                _logger.LogInformation("Seeding default conference...");

                var conference = new Conference(
                    id: Guid.NewGuid(),
                    name: "Octacom Annual Conference 2026",
                    totalCapacity: 100
                );

                await _context.Conferences.AddAsync(conference);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Default conference seeded successfully with Id: {Id}", conference.Id);
            }
            else
            {
                _logger.LogInformation("Conference already exists. Skipping seeding.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }
}