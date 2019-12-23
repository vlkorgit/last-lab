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
    [Route("/getimg")]
    public class GetImgController : Controller
    {
        NeuralImage nm;
        public GetImgController(IRecognizer recognizer)
        {
            nm = recognizer.NeuralImage;
        }
        [HttpGet]
        public string handler(int idx = -1, string _class = "empty")
        {
            Console.WriteLine("getimg");
            Console.WriteLine(idx + " " + _class);
            if (idx < 0 || _class == "empty") return null;
            var img = nm.DbProvider.cant_name_it(_class, idx);
            var b64 = Convert.ToBase64String(img.Bitmap.Blob);
            var ext = Path.GetExtension(img.FullName);
            var ret = new API.Image() { Filename = img.FullName, Class = img.Class, Blob = b64, Extension = ext, Probability = img.Probability };
            return JsonConvert.SerializeObject(ret);
        }
    }
}