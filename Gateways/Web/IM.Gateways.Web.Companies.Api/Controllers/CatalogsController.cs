using Microsoft.AspNetCore.Mvc;

namespace IM.Gateways.Web.Companies.Api.Controllers
{
    [ApiController, Route("api/[controller]")]
    public class CatalogsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
