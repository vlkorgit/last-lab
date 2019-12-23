using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Model;
using Newtonsoft.Json;

namespace ASPServer.Controllers
{
    [ApiController]
    [Route("clear")]
    public class ClearController : Controller
    {
        private NeuralImage neuralImage;
        public ClearController(IRecognizer recognizer)
        {
            neuralImage = recognizer.NeuralImage;
        }
        [HttpGet]
        public string clear()
        {
            Console.WriteLine("Clear request");
            neuralImage.clearDB();
            var stat = neuralImage.getClassStat();
            return JsonConvert.SerializeObject(stat);
        }
    }
}