using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading;

namespace ConsoleApp
{
    class Program
    {
        //postasync может принимать токен
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            HttpClient h = new HttpClient();
            string s = h.GetStringAsync("http://localhost:5000/home").Result;
            var a = JsonConvert.DeserializeObject<API.Message>(s);
            Console.WriteLine(a.Msg);


            //var content = new StringContent(JsonConvert.SerializeObject(new API.Message() { Msg = "Hello" }));
            while (true)
            {
                string enter = Console.ReadLine();
                var content = new MultipartFormDataContent();
                content.Add(new StringContent(enter));
                var answer = h.PostAsync("http://localhost:5000/home", content).Result;
                //a = JsonConvert.DeserializeObject<API.Message>(answer.Content.ReadAsStringAsync().Result);
                string str = answer.Content.ReadAsStringAsync().Result;
                Console.WriteLine(str);
            }
        }
    }
}
