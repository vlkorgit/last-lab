using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace ASPServer.Controllers
{
    [Controller]
    [Route("/test")]
    public class TestController : Controller
    {
        [HttpGet]
        public IActionResult viewget()
        {
            return View("Views/Test.cshtml");
        }
    }
}