using System;
using System.Collections.Generic;
using Caliburn.Micro;
using System.ServiceModel;
using System.Text;
using Iit.Fibertest.Utils35;
using Iit.Fibertest.WpfCommonViews;
using WcfTestBench.MonitoringSettings;

namespace WcfTestBench
{
    public class ShellViewModel : Screen, IShell
    {
        private IniFile _clientIniFile;
        private Logger35 _clientLog;

        public ShellViewModel()
        {
            _clientIniFile = new IniFile();
            _clientIniFile.AssignFile("Client.ini");

            _clientLog = new Logger35();
            _clientLog.AssignFile("Client.log");

            // if there are more than one child view - delete this line
            Temp();
        }

        public void WcfView()
        {
            var vm = new WcfClientViewModel(_clientIniFile, _clientLog);
            IWindowManager windowManager = new WindowManager();
            windowManager.ShowDialog(vm);
        }

        public void Temp()
        {
            var vm = new MonitoringSettingsViewModel(PopulateModel());
            IWindowManager windowManager = new WindowManager();
            windowManager.ShowDialog(vm);

        }

        private MonitoringSettingsModel PopulateModel()
        {
            var model = new MonitoringSettingsModel()
            {
                IsMonitoringOn = true,
                Timespans = new MonitoringTimespans()
                {
                    PreciseMeas = TimeSpan.FromSeconds(3600),
                    PreciseSave = TimeSpan.FromMinutes(60),
                    FastSave = TimeSpan.FromHours(1)
                },
                Charons = new List<MonitoringCharonModel>()
                {
                    new MonitoringCharonModel() { Title = "Грушаука 214", Ports = PopulatePorts(28)},
                    new MonitoringCharonModel() { Title = "192.168.96.57:11834", Ports = PopulatePorts(16)},
                    new MonitoringCharonModel() { Title = "192.168.96.57:11835", Ports = PopulatePorts(4)}
                }
            };
            return model;
        }

        private static List<MonitoringPortModel> PopulatePorts(int count)
        {
            Random gen = new Random();

            var result = new List<MonitoringPortModel>();
            for (int i = 1; i <= count; i++)
            {
                var port = new MonitoringPortModel()
                {
                    PortNumber = i,
                    TraceTitle = new StringBuilder().Insert(0, "Probability is a quite long word ", gen.Next(4)+1).ToString() + $" p{i}",
                    IsIncluded = gen.Next(100) <= 25,
                };
                if (port.IsIncluded || gen.Next(100) <= 75)
                {
                    port.PreciseBaseSpan = TimeSpan.FromSeconds(gen.Next(100) + 15);
                    port.FastBaseSpan = TimeSpan.FromSeconds(gen.Next(100) + 15);
                    if (gen.Next(100) <= 2)
                        port.AdditionalBaseSpan = TimeSpan.FromSeconds(gen.Next(100));
                }
                result.Add(port);
            }
            return result;
        }
    }
}