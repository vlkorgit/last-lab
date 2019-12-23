using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Model;

namespace ASPServer.Controllers
{
    [Route("/stop")]
    [ApiController]
    public class StopController : ControllerBase
    {
        private NeuralImage neuralImage;
        public StopController(IRecognizer recognizer)
        {
            neuralImage = recognizer.NeuralImage;
        }
        [HttpGet]
        public void stop()
        {

        }
    }
}