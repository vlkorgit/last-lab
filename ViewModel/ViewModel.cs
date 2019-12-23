using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using System.Net.Http;
using Newtonsoft.Json;

namespace ViewModel
{
    public class ViewModel : INotifyPropertyChanged
    {
        UI ui;
        private IEnumerable<KeyValuePair<string, int>> stat;
        public IEnumerable<KeyValuePair<string, int>> Stat
        {
            get
            {
                return stat;
            }
            set
            {
                stat = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Stat"));
            }
        }
        private string directory;
        public string Directory
        {
            get
            {
                return directory;
            }
            set
            {
                directory = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Directory"));
            }
        }
        private string test;
        public string Test
        {
            get
            {
                return test;
            }
            set
            {
                test = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Test"));
            }
        }
        public ObservableCollection<Element> Items { get; set; }
        public Hierarchy hierarchy;
        public Hierarchy Hierarchy
        {
            get
            {
                return hierarchy;
            }
            set
            {
                hierarchy = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Hierarchy"));
            }
        }
        private ImageClass selected;
        public ImageClass Selected
        {
            get
            {
                return selected;
            }
            set
            {
                selected = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Selected"));
            }
        }
        public ViewModel(UI ui)
        {
            this.ui = ui;
            Items = new ObservableCollection<Element>();
            Hierarchy = new Hierarchy();
            InitializeCommands();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void InitializeCommands()
        {
            Open = ui.FactoryCommand(OpenExH, (obj) => true);
            Stop = ui.FactoryCommand(StopExH, StopCan);
            Start = ui.FactoryCommand(StartExH, StartCan);
            GetStat = ui.FactoryCommand(GetStatExH, GetStatCan);
            ClearDB = ui.FactoryCommand(ClearDBExH, ClearDbCan);
        }
        private void ExecuteChanged(object sender, EventArgs e)
        {
            //CommandManager.InvalidateRequerySuggested();
        }

        public ICommand Open { get; set; }
        public ICommand Stop { get; set; }
        public ICommand Start { get; set; }
        public ICommand GetStat { get; set; }
        public ICommand ClearDB { get; set; }
        private void ClearDBExH(object obj)
        {
            Task.Factory.StartNew(() =>
            {
                status = Status.DB;
                using (HttpClient client = new HttpClient())
                {
                    //эти вызовы должны быть синхронными
                    try
                    {
                        var result = client.GetAsync("http://localhost:5000/clear").Result;
                        var str = result.Content.ReadAsStringAsync().Result;
                        var buf = JsonConvert.DeserializeObject<Dictionary<string, int>>(str);
                        ui.PlsDoItDispatcher(() => Stat = buf);
                    }
                    catch
                    {
                        Debug.WriteLine("CLEAR SERVER DOESN'T RESPONSE");
                        ui.Message("Server doesn't response");
                    }
                }
                status = Status.IDLE;
            });
        }
        private bool ClearDbCan(object obj)
        {
            if (status == Status.IDLE) return true;
            return false;
        }
        private void GetStatExH(object obj)
        {
            Task.Factory.StartNew(() =>
            {
                status = Status.DB;
                using (HttpClient client = new HttpClient())
                {
                    //эти вызовы должны быть синхронными
                    try
                    {
                        var result = client.GetAsync("http://localhost:5000/statistics").Result;
                        var str = result.Content.ReadAsStringAsync().Result;
                        var buf = JsonConvert.DeserializeObject<Dictionary<string, int>>(str);
                        ui.PlsDoItDispatcher(() => Stat = buf);
                    }
                    catch
                    {
                        Debug.WriteLine("STATISTICS NO RESPONSE");
                        ui.Message("Server doesn't response");
                    }
                }
                status = Status.IDLE;
            });
        }
        private bool GetStatCan(object obj)
        {
            if (status == Status.IDLE) return true;
            return false;
        }
        private void OpenExH(object obj)
        {
            string str;
            if (ui.GetDirectory(out str))
            {
                Items.Clear();
                hierarchy.Clear();
                Directory = str;
                DirectoryInfo dir = new DirectoryInfo(Directory);
                foreach (var f in dir.GetFiles())
                {
                    ui.PlsDoItDispatcher(() => Items.Add(new Element(f.FullName, "Not recognized", 0)));
                }
            }
        }
        private bool StartCan(object obj)
        {
            if (Directory == null) return false;
            if (status != Status.IDLE) return false;
            return true;
        }
        private void StartExH(object obj)
        {
            if (Directory == null) return;
            status = Status.RECOGNIZE;
            //neuralImage.Stop();
            Hierarchy.Clear();
            Task.Factory.StartNew(() =>
            {
                Debug.WriteLine("RECOGNIZE START");

                //шлем по одной картинке, ибо если слать пачку, то обновить не сможем, пока всю пачку не получим
                using (HttpClient h = new HttpClient())
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(Directory);
                    var files = directoryInfo.GetFiles();
                    Task[] tasks = new Task[files.Length];
                    for (int i = 0; i < files.Length; i++)
                    {
                        byte[] bytes = null;
                        try
                        {
                            bytes = File.ReadAllBytes(files[i].FullName);
                        }
                        catch
                        {
                            Debug.WriteLine("Cannot read file " + files[i].FullName);
                            continue;
                        }
                        string Blobstring = Convert.ToBase64String(bytes);
                        API.RecognizeRequest image = new API.RecognizeRequest() { Filename = files[i].FullName, Blob = Blobstring };
                        string JSONimage = JsonConvert.SerializeObject(image);
                        var content = new MultipartFormDataContent();
                        content.Add(new StringContent(JSONimage));
                        tasks[i] = addAsync("http://localhost:5000/recognize", content, h, files[i].FullName);
                    }
                    Task.WaitAll(tasks);
                }
                Debug.WriteLine("RECOGNIZE END");
                status = Status.IDLE;
            });
        }
        //добавить трай-кетч
        private async Task addAsync(string url, MultipartFormDataContent content, HttpClient h, string filename)
        {
            Debug.WriteLine("rec " + filename);
            foreach (var item in Items)
            {
                if (item.Name == filename)
                {
                    string str = null;
                    HttpResponseMessage response = null;
                    try
                    {
                        response = await h.PostAsync(url, content);
                    }
                    catch
                    {
                        ui.PlsDoItDispatcher(() => item.Class = "NO RESPONSE");
                        Debug.WriteLine("SERVER DOESN'T RESPONSE " + filename);
                        return;
                    }
                    try
                    {
                        str = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<API.RecognizeResult>(str);
                        if (result == null) throw new Exception();
                        ui.PlsDoItDispatcher(() =>
                        {
                            item.Class = result.Class;
                            item.Probability = result.Probability;
                            Hierarchy.Add(result.Class, result.Filename);
                        });
                    }
                    catch
                    {
                        ui.PlsDoItDispatcher(() => item.Class = "NOT CORRECT");
                        Debug.WriteLine("RECIEVED EMPTY MESSAGE " + filename);
                        return;
                    }
                }
            }
            Debug.WriteLine("end " + filename);

        }
        private void StopExH(object obj)
        {
            //neuralImage.Stop();
        }
        private bool StopCan(object obj)
        {
            if (status == Status.RECOGNIZE) return true;
            else return false;
        }
        public enum Status { IDLE, RECOGNIZE, DB }
        Status status;
    }
}
