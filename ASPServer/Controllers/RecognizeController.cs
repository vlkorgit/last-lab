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
using Model;
using System.Threading;

namespace ASPServer.Controllers
{
    [ApiController]
    [Route("recognize")]
    public class RecognizeController : Controller
    {
        private NeuralImage neuralImage; //= new NeuralImage("model.onnx", "answers.txt"); //чтобы не создавалось каждый раз

        public RecognizeController( IRecognizer nm)
        {
            neuralImage = nm.NeuralImage;
        }
        [HttpPost]
        public string postMessage()
        {
            string str="NOTHING";
            if (Request.Form.Count > 1) throw new Exception("COUNT MORE THAN 1");
            NeuralImage.Output output = null;
            foreach (var item in Request.Form)
            {
                str = item.Value;
                var image = JsonConvert.DeserializeObject<API.RecognizeRequest>(str);
                Console.WriteLine(image.Filename);
                byte[] bytes = Convert.FromBase64String(image.Blob);
                try
                {
                    output = neuralImage.processFile(image.Filename, bytes);
                }
                catch
                {

                    Console.WriteLine("MODEL EXCEPTION");
                    return null;
                }
            }
            if (output==null) throw new Exception("EMPTY OUTPUT");
            var result = new API.RecognizeResult() { Class = output.Pairs[0].Key, Filename = output.Filename, Probability = output.Pairs[0].Value };
            return JsonConvert.SerializeObject(result);
        }
    }
}
