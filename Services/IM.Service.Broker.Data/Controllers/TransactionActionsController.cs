using IM.Service.Broker.Data.Models.Dto.Http;
using IM.Service.Broker.Data.Services.DtoServices;
using IM.Service.Common.Net.Models.Dto.Http;

using Microsoft.AspNetCore.Mvc;

using System.Threading.Tasks;

namespace IM.Service.Broker.Data.Controllers;

[ApiController, Route("[controller]")]
public class TransactionActionsController : ControllerBase
{
    private readonly TransactionActionDtoManager manager;
    public TransactionActionsController(TransactionActionDtoManager manager) => this.manager = manager;

    public async Task<ResponseModel<PaginatedModel<TransactionActionGetDto>>> Get(int page = 0, int limit = 0) =>
        await manager.GetAsync(new(page, limit));
}