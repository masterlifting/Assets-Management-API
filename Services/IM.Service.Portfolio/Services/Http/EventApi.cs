using IM.Service.Portfolio.Models.Api.Http;

using System;
using System.Threading.Tasks;
using IM.Service.Shared.Models.Service;
using static IM.Service.Shared.Helpers.ServiceHelper;

namespace IM.Service.Portfolio.Services.Http;

public class EventApi
{
    public Task<PaginationModel<EventGetDto>> GetAsync(string companyId, Paginatior paginatior)
    {
        throw new NotImplementedException();
    }
    public Task<PaginationModel<EventGetDto>> GetAsync(Paginatior paginatior)
    {
        throw new NotImplementedException();
    }
}