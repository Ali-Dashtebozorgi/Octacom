using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Octacom.Domain.Repositories
{
    public interface IConferenceRepository
    {
        Task<Conference?> GetById(Guid id);
        Task<Conference?> GetByIdWithBookings(Guid id);
        Task Add(Conference conference);
        Task Update(Conference conference);
    }
}
