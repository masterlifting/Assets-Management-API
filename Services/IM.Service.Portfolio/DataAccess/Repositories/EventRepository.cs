using IM.Service.Common.Net.RepositoryService;
using IM.Service.Portfolio.DataAccess.Entities;

namespace IM.Service.Portfolio.DataAccess.Repositories;

public class EventRepository : RepositoryHandler<Event, DatabaseContext>
{
    public EventRepository(DatabaseContext context) : base(context)
    {
    }
}