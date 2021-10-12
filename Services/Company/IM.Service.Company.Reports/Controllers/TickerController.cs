using CommonServices.HttpServices;
using CommonServices.Models.Http;

using IM.Service.Company.Reports.DataAccess.Entities;
using IM.Service.Company.Reports.DataAccess.Repository;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using System;
using System.Linq;
using System.Threading.Tasks;

namespace IM.Service.Company.Reports.Controllers
{
    [ApiController, Route("[controller]")]
    public class TickersController : Controller
    {
        private readonly RepositorySet<Ticker> repository;
        public TickersController(RepositorySet<Ticker> repository) => this.repository = repository;

        public async Task<ResponseModel<PaginatedModel<CommonServices.Models.Dto.CompanyReports.TickerGetDto>>> Get(int page = 0, int limit = 0)
        {
            var pagination = new HttpPagination(page, limit);
            var errors = Array.Empty<string>();

            var count = await repository.GetCountAsync();
            var paginatedQuery = repository.GetPaginationQuery(pagination, x => x.Name);

            var sources = repository.GetDbSetBy<SourceType>();

            var result = await paginatedQuery
                .Join(sources, x => x.SourceTypeId, y => y.Id, (x, y)
                    => new CommonServices.Models.Dto.CompanyReports.TickerGetDto(x.Name, y.Name, x.SourceValue))
                .ToArrayAsync();

            return new()
            {
                Errors = errors,
                Data = new()
                {
                    Items = result,
                    Count = count
                }
            };
        }

        [HttpGet("{ticker}")]
        public async Task<ResponseModel<CommonServices.Models.Dto.CompanyReports.TickerGetDto>> Get(string ticker)
        {
            var result = await repository.FindAsync(ticker.ToUpperInvariant());

            if (result is null)
                return new() { Errors = new[] { "ticker not found" } };

            var source = await repository.GetDbSetBy<SourceType>().FindAsync(result.SourceTypeId);

            return new()
            {
                Errors = Array.Empty<string>(),
                Data = new(result.Name, source.Name, result.SourceValue)
            };
        }
    }
}
