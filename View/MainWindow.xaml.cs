using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using ViewModel;

namespace View
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new ViewModel.ViewModel(new UIRealisation(Dispatcher));
        }

    }

    public class UIRealisation : UI
    {
        Dispatcher dispatcher;
        public UIRealisation(Dispatcher dispatcher)
        {
            this.dispatcher = dispatcher;
        }
        //доступ к диспетчеру есть в MainWindow
        //доступ к CommandManager в этом проекте
        public bool GetDirectory(out string path)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    path = dialog.SelectedPath;
                    return true;
                }
                else
                {
                    path = null;
                    return false;
                }
            }

        }
        public void Message(string str1, string str2 = null)
        {
            if (str2 == null) MessageBox.Show(str1);
            else MessageBox.Show(str1, str2);
        }
        public void PlsDoItDispatcher(Action action)
        {
            dispatcher.BeginInvoke(action).Wait();
        }
        public ICommand FactoryCommand(Action<object> action, Predicate<object> predicate)
        {
            return new Command(action, predicate);
        }
    }


}
