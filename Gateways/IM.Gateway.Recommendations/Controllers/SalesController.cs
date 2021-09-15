using CommonServices.Models.Dto.Http;
using Microsoft.AspNetCore.Mvc;

using System.Threading.Tasks;
using IM.Gateway.Recommendations.Models.Dto;
using IM.Gateway.Recommendations.Services.DtoServices;

namespace IM.Gateway.Recommendations.Controllers
{
    [ApiController, Route("api/[controller]")]
    public class SalesController : Controller
    {
        public IActionResult Index()
        {
            return Ok();
        }
    }
}
