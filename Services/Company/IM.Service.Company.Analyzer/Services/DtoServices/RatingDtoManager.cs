using IM.Service.Common.Net.HttpServices;
using IM.Service.Common.Net.Models.Dto.Http;
using IM.Service.Common.Net.Models.Dto.Http.CompanyServices;
using IM.Service.Company.Analyzer.DataAccess.Entities;
using IM.Service.Company.Analyzer.DataAccess.Repository;

using Microsoft.EntityFrameworkCore;

using System.Linq;
using System.Threading.Tasks;

namespace IM.Service.Company.Analyzer.Services.DtoServices;

public class RatingDtoManager
{
    private readonly Repository<Rating> ratingRepository;
    private readonly Repository<DataAccess.Entities.Company> companyRepository;

    public RatingDtoManager(
        Repository<Rating> ratingRepository,
        Repository<DataAccess.Entities.Company> companyRepository)
    {
        this.ratingRepository = ratingRepository;
        this.companyRepository = companyRepository;
    }

    public async Task<ResponseModel<RatingGetDto>> GetAsync(int place)
    {
        var rating = await ratingRepository
            .GetQuery()
            .OrderByDescending(x => x.Result)
            .Skip(place - 1)
            .Take(1)
            .FirstAsync();
        
        var company = await companyRepository.FindAsync(rating.CompanyId);

        if (company is not null)
            return new()
            {
                Data = new()
                {
                    Company = company.Name,
                    Place = place,
                    Result = rating.Result,
                    ResultPrice = rating.ResultPrice,
                    ResultReport = rating.ResultReport,
                    ResultCoefficient = rating.ResultCoefficient,
                    UpdateTime = rating.Date
                }
            };

        return new()
        {
            Errors = new[] { "company not found" }
        };
    }
    public async Task<ResponseModel<RatingGetDto>> GetAsync(string companyId)
    {
        var company = await companyRepository.FindAsync(companyId.ToUpperInvariant());

        if (company is null)
            return new()
            {
                Errors = new[] { "company not found" }
            };

        var orderedRatingIds = await ratingRepository
            .GetQuery()
            .OrderByDescending(x => x.Result)
            .Select(x => x.Id)
            .ToArrayAsync();

        var places = orderedRatingIds
            .Select((x, i) => new { Place = i + 1, RatingId = x })
            .ToDictionary(x => x.RatingId, y => y.Place);

        var result = await ratingRepository.FindAsync(x => x.CompanyId == company.Id);

        if (result is null)
            return new()
            {
                Errors = new[] { "rating not found" }
            };

        return new()
        {
            Data = new()
            {
                Company = company.Name,
                Place = places[result.Id],
                Result = result.Result,
                ResultPrice = result.ResultPrice,
                ResultReport = result.ResultReport,
                ResultCoefficient = result.ResultCoefficient,
                UpdateTime = result.Date
            }
        };
    }
    public async Task<ResponseModel<PaginatedModel<RatingGetDto>>> GetAsync(HttpPagination pagination)
    {
        var count = await ratingRepository.GetCountAsync();
        var orderedCompanyIds = await ratingRepository
            .GetQuery()
            .OrderByDescending(x => x.Result)
            .Select(x => x.CompanyId)
            .ToArrayAsync();

        var places = orderedCompanyIds.Select((x, i) => new { Place = i + 1, CompanyId = x });

        var queryResult = await ratingRepository.GetPaginationQueryDesc(pagination, x => x.Result)
            .Join(companyRepository.GetDbSet(),
                x => x.CompanyId,
                y => y.Id,
                (x, y) => new
                {
                    Company = y.Name,
                    x.CompanyId,
                    x.Result,
                    x.ResultPrice,
                    x.ResultReport,
                    x.ResultCoefficient,
                    UpdateTime = x.Date
                })
            .ToArrayAsync();

        var result = queryResult.Join(places,
                x => x.CompanyId,
                y => y.CompanyId,
                (x, y) => new RatingGetDto
                {
                    Place = y.Place,
                    Company = x.Company,
                    Result = x.Result,
                    ResultPrice = x.ResultPrice,
                    ResultReport = x.ResultReport,
                    ResultCoefficient = x.ResultCoefficient,
                    UpdateTime = x.UpdateTime
                })
            .ToArray();


        return new()
        {
            Data = new()
            {
                Items = result,
                Count = count
            }
        };
    }
}