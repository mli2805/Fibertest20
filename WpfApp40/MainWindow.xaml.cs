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

namespace WpfApp40
{
    public class X
    {
        public Task<string> Blah { get; set; }
        private TaskCompletionSource<string> tcs;
        public X()
        {
            tcs = new TaskCompletionSource<string>();
            Blah = tcs.Task;
        }

        public async void InitializeRtuAsync()
        {
            int i = 0;
            while (true)
            {
                i++;
                await TaskEx.Delay(10);
                Console.WriteLine(i);
                if (i == 200)
                {
                    tcs.SetResult("Rtu initialized");
                }
            }
        }
    }/// <summary>
     /// Interaction logic for MainWindow.xaml
     /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
    }
}
