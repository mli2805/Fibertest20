﻿using Caliburn.Micro;
using ClientWcfServiceLibrary;
using System.ServiceModel;
using Iit.Fibertest.Utils35;

namespace DirectRtuClient
{
    public class ShellViewModel : PropertyChangedBase, IShell
    {

        public string IpAddress { get; set; }

        private readonly LogFile _rtuLogger;
        private static IniFile _iniFile35;

        internal static ServiceHost MyServiceHost;
        public ShellViewModel()
        {
            _rtuLogger = new LogFile();
            _rtuLogger.AssignFile(@"rtu.log");

            _iniFile35 = new IniFile();
            _iniFile35.AssignFile(@"rtu.ini");

            IpAddress = _iniFile35.Read(IniSection.General, IniKey.OtauIp, @"192.168.96.53");

            StartWcf();

        }

        private void StartWcf()
        {
            MyServiceHost?.Close();
            ClientWcfService.ClientLog = _rtuLogger;
            MyServiceHost = new ServiceHost(typeof(ClientWcfService));
            MyServiceHost.Open();
        }

        public void OtdrView()
        {
            var vm = new OtdrViewModel(_iniFile35, _rtuLogger, IpAddress);
            IWindowManager windowManager = new WindowManager();
            windowManager.ShowWindow(vm);
        }
        public void OtauView()
        {
            var otauPort = _iniFile35.Read(IniSection.General, IniKey.OtauPort, 23);
            var vm = new OtauViewModel(IpAddress, otauPort, _iniFile35, _rtuLogger);
            IWindowManager windowManager = new WindowManager();
            windowManager.ShowWindow(vm);
        }
    }
}