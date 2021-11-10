using IM.Service.Common.Net.HttpServices;
using IM.Service.Common.Net.Models.Dto.Http;

using IM.Service.Common.Net.Models.Dto.Http.Companies;
using Microsoft.AspNetCore.Mvc;

using System.Threading.Tasks;
using IM.Service.Company.Models.Dto;
using IM.Service.Company.Services.DtoServices;

namespace IM.Service.Company.Controllers
{
    [ApiController, Route("[controller]")]
    public class CompaniesController : ControllerBase
    {
        private readonly CompanyDtoManager manager;
        public CompaniesController(CompanyDtoManager manager) => this.manager = manager;

        public async Task<ResponseModel<PaginatedModel<CompanyGetDto>>> Get(int page = 0, int limit = 0) =>
            await manager.GetAsync((HttpPagination) new(page, limit));

        [HttpGet("{companyId}")]
        public async Task<ResponseModel<CompanyGetDto>> Get(string companyId) => await manager.GetAsync(companyId);

        [HttpPost]
        public async Task<ResponseModel<string>> Post(CompanyPostDto company) => await manager.CreateAsync(company);
        
        [HttpPut("{companyId}")]
        public async Task<ResponseModel<string>> Put(string companyId, CompanyPutDto company) => await manager.UpdateAsync(companyId, company);
        
        [HttpDelete("{companyId}")]
        public async Task<ResponseModel<string>> Delete(string companyId) => await manager.DeleteAsync(companyId);
    }
}
