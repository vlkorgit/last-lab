using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewModel
{
    public class Hierarchy : ObservableCollection<ImageClass>
    {
        public void Add(string class_name, string image_name)
        {
            bool flag = false;
            foreach (var item in this)
            {
                if (item.Compare(class_name))
                {
                    item.AddImg(image_name);
                    flag = true;
                }
            }
            if (!flag)
            {
                this.Add(new ImageClass(class_name, image_name));
            }
        }
    }
    public class ImageClass : INotifyPropertyChanged
    {
        private int count = 0;
        public int Count
        {
            get
            {
                return count;
            }
            set
            {
                count = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Count"));
            }
        }
        public ImageClass(string name, string image_name)
        {
            Name = name;
            Images = new ObservableCollection<string>();
            Images.Add(image_name);
            Count = Images.Count;
        }
        private string name;
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Name"));
            }
        }
        public void AddImg(string image)
        {
            Images.Add(image);
            Count = Images.Count;
        }
        public ObservableCollection<string> Images { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public bool Compare(string str)
        {
            if (Name == str) return true;
            return false;
        }

    }
}
