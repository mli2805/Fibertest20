﻿using System;
using System.Threading;
using System.Windows;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class LeaveAppViewModel : Screen
    {
        public string Line1 { get; set; }
        public string Line2 { get; set; }
      
        public void Initialize(UnRegisterReason reason, string username)
        {
            switch (reason)
            {
                case UnRegisterReason.DbOptimizationStarted:
                    Line1 = Resources.SID_Database_optimization_started_on_server_;
                    Line2 = Resources.SID_Please_leave_application_;
                    return;
                case UnRegisterReason.DbOptimizationFinished:
                    Line1 = Resources.SID_Database_optimization_is_terminated_successfully_;
                    Line2 = Resources.SID_Please_restart_application_;
                    return;
                case UnRegisterReason.UserRegistersAnotherSession:
                    Line1 = string.Format(Resources.SID_User__0__is_logged_in_from_a_different_device_at__1_, username, DateTime.Now);
                    Line2 = Resources.SID_Please_leave_application_;
                    return;
                default: return;
            }
        }

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
