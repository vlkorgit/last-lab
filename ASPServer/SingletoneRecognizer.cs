using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ASPServer
{
    //нужно, чтобы один раз создать этот класс и передавать его в контроллер
    public interface IRecognizer
    {
        NeuralImage NeuralImage { get; }
    }
    public class SingletoneRecognizer : IRecognizer
    {
        public SingletoneRecognizer()
        {
            NeuralImage = new NeuralImage("model.onnx", "answers.txt");
        }
        public NeuralImage NeuralImage
        {
            get;
        }
    }
}
