using System;
using System.Threading;
using System.Windows;
using Caliburn.Micro;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class LeaveAppViewModel : Screen
    {
        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Information;
        }

        public void ExitApp()
        {
            Thread.Sleep(TimeSpan.FromSeconds(1));
            Application.Current.Dispatcher?.InvokeAsync(() => Application.Current.Shutdown());
        }
    }
}
