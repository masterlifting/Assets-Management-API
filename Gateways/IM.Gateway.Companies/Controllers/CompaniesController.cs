using CommonServices.HttpServices;
using CommonServices.Models.Http;

using IM.Gateway.Companies.Models.Dto;
using IM.Gateway.Companies.Services.DtoServices;

using Microsoft.AspNetCore.Mvc;

using System.Threading.Tasks;

namespace IM.Gateway.Companies.Controllers
{
    [ApiController, Route("api/[controller]")]
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
        public async Task<ResponseModel<string>> Put(string ticker, CompanyPostDto company) => await manager.UpdateAsync(ticker, company);
        
        [HttpDelete("{ticker}")]
        public async Task<ResponseModel<string>> Delete(string ticker) => await manager.DeleteAsync(ticker);
    }
}
