using System;
using System.Threading;
using System.Windows;
using Caliburn.Micro;

namespace Iit.Fibertest.Client
{
    public class LeaveAppViewModel : Screen
    {
        public void ExitApp()
        {
            Thread.Sleep(TimeSpan.FromSeconds(1));
            Application.Current.Dispatcher.InvokeAsync(() => Application.Current.Shutdown());
        }
    }
}
