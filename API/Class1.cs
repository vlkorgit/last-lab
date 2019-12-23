using System;
using System.Collections.Generic;

namespace API
{
    public class Message
    {
        public string Msg { get; set; }
    }
    //send this
    public class RecognizeRequest
    {
        public string Filename { get; set; }
        //base64
        public string Blob { get; set; }
    }
    //recieve this
    public class RecognizeResult
    {
        public string Filename { get; set; }
        public string Class { get; set; }
        public float Probability { get; set; }
    }
    public class Image
    {
        public string Filename { get; set; }
        public string Class { get; set; }
        public float Probability { get; set; }
        //base64
        public string Blob { get; set; }
        public string Extension { get; set; }
    }
}
