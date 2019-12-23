using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using System.Diagnostics;
namespace View
{
    public class Command : ICommand
    {
        private Predicate<object> _canExecute;
        private Action<object> _execute;
        public Command(Action<object> action, Predicate<object> predicate)
        {
            _execute = action;
            _canExecute = predicate;
            CanExecuteChanged += (obj, e) => CommandManager.InvalidateRequerySuggested();
        }
        public void setCanExecute(Predicate<object> obj)
        {
            _canExecute = obj;
        }
        public void setExecute(Action<object> obj)
        {
            _execute = obj;
        }
        public event EventHandler CanExecuteChanged
        {
            //менеджера команд нет во вьюмодел, в net core
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute(parameter);
        }
        public void Execute(object parameter)
        {
            _execute(parameter);
        }
    }
}
