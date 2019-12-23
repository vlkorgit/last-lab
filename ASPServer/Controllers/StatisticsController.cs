using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Model;
using Newtonsoft.Json;

namespace ASPServer.Controllers
{
    [ApiController]
    [Route("statistics")]
    public class StatisticsController : Controller
    {
        private NeuralImage nm;
        public StatisticsController(IRecognizer recognizer)
        {
            nm = recognizer.NeuralImage;
        }
        [HttpGet]
        public string getStat()
        {
            Console.WriteLine("Get statistics request");
            var stat = nm.getClassStat();
            return JsonConvert.SerializeObject(stat);
        }
    }
}