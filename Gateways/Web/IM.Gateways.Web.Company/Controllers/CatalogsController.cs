using Microsoft.AspNetCore.Mvc;

namespace IM.Gateways.Web.Company.Controllers
{
    [ApiController, Route("api/[controller]")]
    public class CatalogsController : Controller
    {
        public IActionResult Index()
        {
            return Ok();
        }
    }
}
