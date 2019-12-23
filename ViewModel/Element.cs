using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewModel
{
    public class Element : INotifyPropertyChanged
    {
        private string name = null;
        private string _class = null;
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
        public string Class
        {
            get
            {
                return _class;
            }
            set
            {
                _class = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Class"));
            }
        }
        private float probability;
        public float Probability
        {
            get
            {
                return probability;
            }
            set
            {
                probability = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Probability"));
            }
        }

        public Element(string name, string __class, float probability)
        {
            this.name = name;
            this._class = __class;
            this.probability = probability;
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
