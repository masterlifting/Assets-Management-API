using DataSetter.Clients;

using IM.Service.Common.Net.Models.Dto.Http;
using IM.Service.Common.Net.Models.Dto.Http.Companies;

using Microsoft.AspNetCore.Mvc;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataSetter.Controllers
{
    [ApiController, Route("[controller]")]
    public class CompanyPricesController : ControllerBase
    {
        private readonly CompanyDataClient client;
        public CompanyPricesController(CompanyDataClient client) => this.client = client;

        [HttpPost]
        public async Task<ResponseModel<string>> Post(PricePostDto model) => await client.Post("companies", model);
        [HttpPost("collection/")]
        public async Task<ResponseModel<string>> Post(IEnumerable<PricePostDto> models) => await client.Post("companies", models);

        [HttpPut("{companyId}/{Year:int}/{Month:int}/{Day:int}")]
        public async Task<ResponseModel<string>> Put(string companyId, int year, int month, int day, PricePutDto model) => await client.Put("companies", model, companyId);
        [HttpDelete("{companyId}/{Year:int}/{Month:int}/{Day:int}")]
        public async Task<ResponseModel<string>> Delete(string companyId, int year, int month, int day) => await client.Delete("companies", companyId);
    }
}
