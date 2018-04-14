using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;

namespace Iit.Fibertest.Client
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

            String thisprocessname = Process.GetCurrentProcess().ProcessName;

            if (Process.GetProcesses().Count(p => p.ProcessName == thisprocessname) > 1)
                throw new Exception();
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
