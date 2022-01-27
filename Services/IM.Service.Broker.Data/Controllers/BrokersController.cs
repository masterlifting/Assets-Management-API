using IM.Service.Broker.Data.Models.Dto.Http;
using IM.Service.Broker.Data.Services.DtoServices;
using IM.Service.Common.Net.Models.Dto.Http;

using Microsoft.AspNetCore.Mvc;

using System.Threading.Tasks;

namespace IM.Service.Broker.Data.Controllers;

[ApiController, Route("[controller]")]
public class BrokersController : ControllerBase
{
    private readonly BrokerDtoManager manager;
    public BrokersController(BrokerDtoManager manager) => this.manager = manager;

    public async Task<ResponseModel<PaginatedModel<BrokerGetDto>>> Get(int page = 0, int limit = 0) =>
        await manager.GetAsync(new(page, limit));
}