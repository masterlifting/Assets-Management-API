using IM.Service.Broker.Data.DataAccess.Repository;
using IM.Service.Broker.Data.Models.Dto;
using IM.Service.Common.Net.HttpServices;
using IM.Service.Common.Net.Models.Dto.Http;

using Microsoft.EntityFrameworkCore;

using System.Linq;
using System.Threading.Tasks;

namespace IM.Service.Broker.Data.Services.DtoServices;

public class BrokerDtoManager
{
    private readonly Repository<DataAccess.Entities.Broker> brokerRepository;

    public BrokerDtoManager(Repository<DataAccess.Entities.Broker> brokerRepository)
    {
        this.brokerRepository = brokerRepository;
    }
    public async Task<ResponseModel<PaginatedModel<BrokerGetDto>>> GetAsync(HttpPagination pagination)
    {
        var count = await brokerRepository.GetCountAsync();
        var paginatedResult = brokerRepository.GetPaginationQuery(pagination, x => x.Name);

        var items = await paginatedResult.Select(x => new BrokerGetDto
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