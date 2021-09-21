using Microsoft.AspNetCore.Mvc;

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
