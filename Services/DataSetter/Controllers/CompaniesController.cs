using IM.Service.Common.Net.Models.Dto.Http;

using Microsoft.AspNetCore.Mvc;

using System.Threading.Tasks;
using DataSetter.Clients;
using DataSetter.Models.Dto;

namespace DataSetter.Controllers
{
    [ApiController, Route("[controller]")]
    public class CompaniesController : ControllerBase
    {
        private readonly CompanyClient client;
        public CompaniesController(CompanyClient client) => this.client = client;

        [HttpPost]
        public async Task<ResponseModel<string>> Post(CompanyPostDto model) => await client.Post("companies", model);

        [HttpPut("{companyId}")]
        public async Task<ResponseModel<string>> Put(string companyId, CompanyPutDto model) => await client.Put("companies", model, companyId);

        [HttpDelete("{companyId}")]
        public async Task<ResponseModel<string>> Delete(string companyId) => await client.Delete("companies", companyId);
    }
}
