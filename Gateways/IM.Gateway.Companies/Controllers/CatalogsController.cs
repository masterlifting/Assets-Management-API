using Microsoft.AspNetCore.Mvc;

namespace IM.Gateway.Companies.Controllers
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
