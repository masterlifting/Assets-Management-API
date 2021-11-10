using DataSetter.Clients;

using IM.Service.Common.Net.Models.Dto.Http;
using IM.Service.Common.Net.Models.Dto.Http.Companies;

using Microsoft.AspNetCore.Mvc;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataSetter.Controllers
{
    [ApiController, Route("[controller]")]
    public class CompanyReportsController : ControllerBase
    {
        private readonly CompanyDataClient client;
        public CompanyReportsController(CompanyDataClient client) => this.client = client;

        [HttpPost]
        public async Task<ResponseModel<string>> Post(ReportPostDto model) => await client.Post("companies", model);
        [HttpPost("collection/")]
        public async Task<ResponseModel<string>> Post(IEnumerable<ReportPostDto> models) => await client.Post("companies", models);

        [HttpPut("{companyId}/{Year:int}/{Quarter:int}")]
        public async Task<ResponseModel<string>> Put(string companyId, int year, int quarter, ReportPutDto model) => await client.Put("companies", model, companyId);

        [HttpDelete("{companyId}/{Year:int}/{Quarter:int}")]
        public async Task<ResponseModel<string>> Delete(string companyId, int year, int quarter) => await client.Delete("companies", companyId);
    }
}
