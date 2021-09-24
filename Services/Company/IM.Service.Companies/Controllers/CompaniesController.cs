using CommonServices.HttpServices;
using CommonServices.Models.Http;

using CommonServices.Models.Dto.GatewayCompanies;
using Microsoft.AspNetCore.Mvc;

using System.Threading.Tasks;
using IM.Service.Companies.Services.DtoServices;

namespace IM.Service.Companies.Controllers
{
    [ApiController, Route("[controller]")]
    public class CompaniesController : ControllerBase
    {
        private readonly DtoCompanyManager manager;
        public CompaniesController(DtoCompanyManager manager) => this.manager = manager;

        public async Task<ResponseModel<PaginatedModel<CompanyGetDto>>> Get(int page = 0, int limit = 0) =>
            await manager.GetAsync((HttpPagination) new(page, limit));

        [HttpGet("{ticker}")]
        public async Task<ResponseModel<CompanyGetDto>> Get(string ticker) => await manager.GetAsync(ticker);
        

        [HttpPost]
        public async Task<ResponseModel<string>> Post(CompanyPostDto company) => await manager.CreateAsync(company);
        
        [HttpPut("{ticker}")]
        public async Task<ResponseModel<string>> Put(string ticker, CompanyPutDto company) => await manager.UpdateAsync(ticker, company);
        
        [HttpDelete("{ticker}")]
        public async Task<ResponseModel<string>> Delete(string ticker) => await manager.DeleteAsync(ticker);
    }
}
