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
    [Route("/class")]
    public class GetClassesController : Controller
    {
        public NeuralImage nm;
        public GetClassesController(IRecognizer recognizer)
        {
            nm = recognizer.NeuralImage;
        }
        [HttpGet]
        public string handler(string _class = null)
        {
            if (_class == null) return "empty";
            var q=  nm.DbProvider.getImagesByClass(_class);
            if (q == null) return "empty";
            var lst = new List<API.Image>();
            foreach (var i in q)
            {
                lst.Add(new API.Image()
                {
                    Filename = i.FullName,
                    Class = i.Class,
                    Extension = Path.GetExtension(i.FullName),
                    Probability = i.Probability,
                    Blob = Convert.ToBase64String(i.Bitmap.Blob)
                });
            }
            return JsonConvert.SerializeObject(lst.ToArray());
        }
    }
}