using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ASPServer.Models;
using Newtonsoft.Json;
using System.Net.Http;

namespace ASPServer.Controllers
{
    [ApiController]
    [Route("/home")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
        [HttpPost]
        public string postMessage()
        {
            Console.WriteLine("POST");
            Console.WriteLine(Request.Scheme);
            string str="NOTHING";
            foreach (var item in Request.Form)
            {
                Console.WriteLine(item.Value);
                str = item.Value;
            }
            return str;
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
