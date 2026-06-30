using Microsoft.EntityFrameworkCore;
using Octacom.Domain;
using Octacom.Domain.Repositories;

namespace Octacom.Infrastructure;

public class ConferenceRepository : IConferenceRepository
{
    private readonly ConferenceDbContext _context;

    public ConferenceRepository(ConferenceDbContext context)
    {
        _context = context;
    }

    public async Task<Conference?> GetById(Guid id)
    {
        return await _context.Conferences
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Conference?> GetByIdWithBookings(Guid id)
    {
        return await _context.Conferences
            .Include(c => c.Bookings)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task Add(Conference conference)
    {
        await _context.Conferences.AddAsync(conference);
        await _context.SaveChangesAsync();
    }

    public async Task Update(Conference conference)
    {
        _context.Conferences.Update(conference);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Conference>> GetAll()
    {
        return await _context.Conferences.ToListAsync();
    }
}