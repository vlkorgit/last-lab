using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ViewModel
{
    public interface UI
    {
        void Message(string str1, string str2 = null);
        bool GetDirectory(out string path);
        void PlsDoItDispatcher(Action action);
        ICommand FactoryCommand(Action<object> execute, Predicate<object> can_execute);
    }
}
