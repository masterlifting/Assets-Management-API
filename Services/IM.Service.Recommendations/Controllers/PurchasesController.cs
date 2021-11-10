using Microsoft.AspNetCore.Mvc;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IM.Service.Recommendations.Controllers
{
    [ApiController, Route("api/[controller]")]
    public class PurchasesController : Controller
    {
        public IActionResult Index()
        {
            return Ok();
        }
    }
}
