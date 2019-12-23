using Microsoft.ML.OnnxRuntime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Microsoft.ML.OnnxRuntime.Tensors;
using System.Threading;
using System.Collections.Concurrent;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using Database;
namespace Model
{

    public class NeuralImage : IDisposable
    {
        ArrayList answers = new ArrayList();
        Exception exception = null;
        InferenceSession session = null;
        public DBprovider DbProvider { get; }
        CancellationTokenSource cts;

        public NeuralImage(string model_path, string answer_path)
        {
            Console.WriteLine("INITIALIZE neuralImage");
            try
            {
                using (StreamReader sr = new StreamReader(answer_path))
                {
                    while (!sr.EndOfStream)
                    {
                        answers.Add(sr.ReadLine());
                    }
                    sr.Close();
                }
                session = new InferenceSession(model_path);
                DbProvider = new DBprovider();

            }
            catch (Exception ex)
            {
                Debug.WriteLine("MODEL OR ANSWERS DIDN'T LOADED");
                exception = ex;
            }
        }
        public Exception getException()
        {
            return exception;
        }
        public void Stop()
        {
            if (cts != null) cts.Cancel();
        }
        public ConcurrentQueue<Output> processDirAsync(string dir_path)
        {
            if (exception != null)
            {
                exception = new Exception("Model was incorrect");
                return null;
            }
            else
            {
                Debug.WriteLine("REC");
                ConcurrentQueue<string> requests = new ConcurrentQueue<string>(); //спросить мб лучше конкурентную использовать, и без блокировок обойтись
                cts = new CancellationTokenSource();
                ConcurrentQueue<Output> returns = new ConcurrentQueue<Output>();
                int proc = System.Environment.ProcessorCount;
                try
                {
                    DirectoryInfo dir = new DirectoryInfo(dir_path);

                    var files = dir.GetFiles();

                    Thread[] threads = new Thread[proc];
                    Thread wait_thread = new Thread(() =>
                    {
                        for (int i = 0; i < proc; i++)
                        {
                            threads[i].Join();
                        }
                        returns.Enqueue(null);
                    });
                    wait_thread.IsBackground = true;
                    foreach (var file in files) requests.Enqueue(file.FullName);
                    for (int i = 0; i < proc; i++)
                    {
                        threads[i] = new Thread(() =>
                        #region scary lambda
                        {
                            string image_path;
                            byte[] blob = null;
                            while (requests.TryDequeue(out image_path))
                            {
                                try
                                {
                                    blob = File.ReadAllBytes(image_path);
                                }
                                catch
                                {
                                    Debug.WriteLine("Cannot read file " + image_path);
                                    continue;
                                }
                                if (DbProvider.isExist(image_path) && DbProvider.compareBlob(image_path, blob))
                                {
                                    Debug.WriteLine("ALREADY EXIST " + image_path);
                                    var img = DbProvider.getImage(image_path);
                                    returns.Enqueue(new Output(image_path, new KeyValuePair<string, float>(img.Class, img.Probability)));
                                    continue;
                                }
                                if (cts.Token.IsCancellationRequested) return;

                                var inputMeta = session.InputMetadata;

                                var container = new List<NamedOnnxValue>();
                                float[] inputData;
                                //inputdata!=blob
                                if ((inputData = CustomImage.parseImage(image_path, blob)) == null) return;

                                if (cts.Token.IsCancellationRequested) return;

                                foreach (var name in inputMeta.Keys)
                                {
                                    var tensor = new DenseTensor<float>(inputData, inputMeta[name].Dimensions);
                                    //Microsoft.ML.OnnxRuntime.Tensors.Tensor
                                    container.Add(NamedOnnxValue.CreateFromTensor<float>(name, tensor));
                                }

                                if (cts.Token.IsCancellationRequested) return;

                                using (var results = session.Run(container))
                                {
                                    foreach (var r in results)
                                    {
                                        var list = new List<KeyValuePair<string, float>>();
                                        int iter = 0;
                                        foreach (var a in r.AsEnumerable<float>())
                                        {
                                            list.Add(new KeyValuePair<string, float>(answers[iter].ToString(), a));
                                            iter++;
                                        }

                                        var smthg = (from pair in list
                                                     orderby pair.Value descending
                                                     select pair).Take(3);
                                        var buf = new Output(image_path, smthg.ToArray());
                                        returns.Enqueue(buf);
                                        DbProvider.add(image_path, smthg.First().Key, smthg.First().Value, blob);
                                    }
                                }

                            }
                        }
                        #endregion
                        );
                        threads[i].Start();
                    }
                    wait_thread.Start();
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
                return returns;
            }
        }

        public Output processFile(string image_path, byte[] bytes)
        {
            if (DbProvider.isExist(image_path) && DbProvider.compareBlob(image_path, bytes))
            {
                Debug.WriteLine("ALREADY EXIST " + image_path);
                var img = DbProvider.getImage(image_path);
                return new Output(image_path, new KeyValuePair<string, float>(img.Class, img.Probability));
            }
            //if (cts.Token.IsCancellationRequested) return;

            var inputMeta = session.InputMetadata;

            var container = new List<NamedOnnxValue>();
            float[] inputData;

            //if ((inputData = CustomImage.parseImage(image_path, bytes)) == null) return;
            inputData = CustomImage.parseImage(image_path, bytes);
            //if (cts.Token.IsCancellationRequested) return;

            foreach (var name in inputMeta.Keys)
            {
                var tensor = new DenseTensor<float>(inputData, inputMeta[name].Dimensions);
                //Microsoft.ML.OnnxRuntime.Tensors.Tensor
                container.Add(NamedOnnxValue.CreateFromTensor<float>(name, tensor));
            }

            //if (cts.Token.IsCancellationRequested) return;
            Output output = null;
            using (var results = session.Run(container))
            {
                foreach (var r in results)
                {
                    var list = new List<KeyValuePair<string, float>>();
                    int iter = 0;
                    foreach (var a in r.AsEnumerable<float>())
                    {
                        list.Add(new KeyValuePair<string, float>(answers[iter].ToString(), a));
                        iter++;
                    }

                    var smthg = (from pair in list
                                 orderby pair.Value descending
                                 select pair).Take(3);
                    DbProvider.add(image_path, smthg.First().Key, smthg.First().Value, bytes);
                    output = new Output(image_path, smthg.ToArray());
                }
            }
            return output;
        }
        public class Output
        {
            public string Filename { get; set; }
            public KeyValuePair<string, float>[] Pairs { get; set; }
            public Output(string filename, params KeyValuePair<string, float>[] pairs)
            {
                this.Filename = filename;
                this.Pairs = pairs;
            }
            public override string ToString()
            {
                string[] arr = Filename.Split('\\');
                string buf = "---->" + arr[arr.Length - 1];
                foreach (var pair in Pairs)
                {
                    buf += "\n" + pair.Key.Substring(10) + " with probability " + pair.Value.ToString("0.00");
                }
                return buf;
            }
        }

        public IEnumerable<KeyValuePair<string, int>> getClassStat()
        {
            if (DbProvider != null)
                return DbProvider.getClassStat();
            else return null;
        }
        public void clearDB()
        {
            if (DbProvider != null)
                DbProvider.clear();
        }
        public void Dispose()
        {
            if (session != null)
            {
                session.Dispose();
                session = null;
            }
            //if (dbprovider != null)
            // dbprovider.Dispose();
        }
    }
}
