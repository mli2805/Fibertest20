using System;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace Iit.Fibertest.TestBench
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        static App()
        {
            AppDomain.CurrentDomain.UnhandledException +=
                (s, e) => Log((Exception)e.ExceptionObject);

        }
        public App()
        {
            InitializeComponent();
            DispatcherUnhandledException += (sender, e) => Log(e.Exception);
        }

        private static void Log(Exception eException)
        {
            File.AppendAllText("crash.txt",
                Environment.NewLine +
                Environment.NewLine +
                Environment.NewLine +
                eException);
            MessageBox.Show(eException.ToString());
        }
    }
}
