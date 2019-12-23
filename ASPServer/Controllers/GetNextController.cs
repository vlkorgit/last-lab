using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Model;
using Newtonsoft.Json;

namespace ASPServer.Controllers
{
    [Controller]
    [Route("/getnext")]
    public class GetNextController : Controller
    {
        private NeuralImage nm;
        public GetNextController(IRecognizer recognizer)
        {
            nm = recognizer.NeuralImage;
        }
        [HttpGet]
        public string getNext()
        {
            Console.WriteLine("getnext");
            var img = nm.DbProvider.getNextImage();
            var b64 = Convert.ToBase64String(img.Bitmap.Blob);
            var ext = Path.GetExtension(img.FullName);
            var rr = new API.Image() { Filename = img.FullName, Class = img.Class,
                Probability = img.Probability, Blob=b64, Extension=ext };
            return JsonConvert.SerializeObject(rr);
        }
    }
}