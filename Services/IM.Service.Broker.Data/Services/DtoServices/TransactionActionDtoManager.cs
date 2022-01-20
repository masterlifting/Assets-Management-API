using IM.Service.Broker.Data.DataAccess.Entities;
using IM.Service.Broker.Data.DataAccess.Repository;
using IM.Service.Broker.Data.Models.Dto;
using IM.Service.Common.Net.HttpServices;
using IM.Service.Common.Net.Models.Dto.Http;

using Microsoft.EntityFrameworkCore;

using System.Linq;
using System.Threading.Tasks;

namespace IM.Service.Broker.Data.Services.DtoServices;

public class TransactionActionDtoManager
{
    private readonly Repository<TransactionAction> repository;

    public TransactionActionDtoManager(Repository<TransactionAction> repository)
    {
        this.repository = repository;
    }
    public async Task<ResponseModel<PaginatedModel<TransactionActionGetDto>>> GetAsync(HttpPagination pagination)
    {
        var count = await repository.GetCountAsync();
        var paginatedResult = repository.GetPaginationQuery(pagination, x => x.Name);

        var items = await paginatedResult.Select(x => new TransactionActionGetDto
        {
                Name = x.Name,
                Description = x.Description
            })
            .ToArrayAsync();

        return new()
        {
            Data = new()
            {
                Items = items,
                Count = count
            }
        };
    }
}