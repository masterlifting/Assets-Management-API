using IM.Service.Portfolio.Models.Api.Http;

using System;
using System.Threading.Tasks;
using IM.Service.Shared.Models.Service;
using static IM.Service.Shared.Helpers.ServiceHelper;

namespace IM.Service.Portfolio.Services.Http;

public class DealApi
{
    public Task<PaginationModel<DealGetDto>> GetAsync(string companyId, Paginatior paginatior)
    {
        throw new NotImplementedException();
    }
    public Task<PaginationModel<DealGetDto>> GetAsync(Paginatior paginatior)
    {
        throw new NotImplementedException();
    }
}