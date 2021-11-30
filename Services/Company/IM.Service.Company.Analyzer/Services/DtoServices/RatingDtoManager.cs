using IM.Service.Common.Net.HttpServices;
using IM.Service.Common.Net.Models.Dto.Http;

using IM.Service.Company.Analyzer.DataAccess.Entities;
using IM.Service.Company.Analyzer.DataAccess.Repository;

using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using IM.Service.Common.Net.Models.Dto.Http.CompanyServices;

namespace IM.Service.Company.Analyzer.Services.DtoServices
{
    public class RatingDtoManager
    {
        private readonly RepositorySet<Rating> ratingRepository;
        private readonly RepositorySet<DataAccess.Entities.Company> companyRepository;

        public RatingDtoManager( 
            RepositorySet<Rating> ratingRepository,
            RepositorySet<DataAccess.Entities.Company> companyRepository)
        {
            this.ratingRepository = ratingRepository;
            this.companyRepository = companyRepository;
        }

        public async Task<ResponseModel<RatingGetDto>> GetAsync(int place)
        {
            var rating = await ratingRepository.FindAsync(place);

            if (rating is null)
                return new()
                {
                    Errors = new[] { "rating not found" }
                };

            var company = await companyRepository.FindAsync(rating.CompanyId);
            
            if (company is null)
                return new()
                {
                    Errors = new[] { "company not found" }
                };

            return new()
            {
                Data = new()
                {
                    Company = company.Name,
                    Place = rating.Place,
                    Result = rating.Result,
                    UpdateTime = rating.UpdateTime
                }
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

            var rating = await ratingRepository.FindAsync(company.Id);
            
            if (rating is null)
                return new()
                {
                    Errors = new[] { "rating not found" }
                };

            return new()
            {
                Data = new()
                {
                    Company = company.Name,
                    Place = rating.Place,
                    Result = rating.Result,
                    UpdateTime = rating.UpdateTime
                }
            };
        }
        public async Task<ResponseModel<PaginatedModel<RatingGetDto>>> GetAsync(HttpPagination pagination)
        {
            var count = await ratingRepository.GetCountAsync();
            var ratingPaginatedResult = ratingRepository.GetPaginationQuery(pagination, x => x.Place);
            var result = await ratingPaginatedResult.Join(companyRepository.GetDbSet(), x => x.CompanyId, y => y.Id, (x, y) =>
                    new RatingGetDto
                    {
                        Company = y.Name,
                        Place = x.Place,
                        Result = x.Result,
                        UpdateTime = x.UpdateTime
                    })
                .ToArrayAsync();

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
}
