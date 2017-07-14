using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;

namespace WcfTestBench.MonitoringSettings
{
    public class MonitoringSettingsViewModel : Screen
    {
        public List<PortLineModel> Ports { get; set; } // for binding
        public string CharonAddress { get; set; }
        public string CycleFullTime { get; set; }

        public MonitoringSettingsViewModel()
        {
            CharonAddress = "192.168.96.000:23";
            Ports = new List<PortLineModel>
            {
                new PortLineModel()
                {
                    PortNumber = 1,
                    TraceTitle =
                        "Very very very long title for quite short trace in Minsk City (testing new MonitoringSettingsView) p1",
                    PreciseBaseSpan = TimeSpan.FromSeconds(73),
                    FastBaseSpan = TimeSpan.FromSeconds(34),
                    IsMonitoringOn = true,
                },
                new PortLineModel()
                {
                    PortNumber = 2,
                    TraceTitle =
                        "Very very very long title for quite short trace in Minsk City (testing new MonitoringSettingsView) p2",
                    PreciseBaseSpan = TimeSpan.FromSeconds(53),
                    FastBaseSpan = TimeSpan.FromSeconds(32),
                    IsMonitoringOn = false,
                },
                new PortLineModel()
                {
                    PortNumber = 3,
                    TraceTitle =
                        "Very very very long title for quite short trace in Minsk City (testing new MonitoringSettingsView) p3",
                    PreciseBaseSpan = TimeSpan.FromSeconds(34),
                    FastBaseSpan = TimeSpan.FromSeconds(34),
                    IsMonitoringOn = true,
                },
                new PortLineModel()
                {
                    PortNumber = 4,
                    TraceTitle =
                        "Very very very long title for quite short trace in Minsk City (testing new MonitoringSettingsView) p4",
                    PreciseBaseSpan = TimeSpan.FromSeconds(34),
                    FastBaseSpan = TimeSpan.FromSeconds(34),
                    IsMonitoringOn = true,
                },
                new PortLineModel()
                {
                PortNumber = 1,
                TraceTitle =
                    "Very very very long title for quite short trace in Minsk City (testing new MonitoringSettingsView) p1",
                PreciseBaseSpan = TimeSpan.FromSeconds(73),
                FastBaseSpan = TimeSpan.FromSeconds(34),
                IsMonitoringOn = true,
            },
            new PortLineModel()
            {
                PortNumber = 2,
                TraceTitle =
                    "Very very very long title for quite short trace in Minsk City (testing new MonitoringSettingsView) p2",
                PreciseBaseSpan = TimeSpan.FromSeconds(53),
                FastBaseSpan = TimeSpan.FromSeconds(32),
                IsMonitoringOn = false,
            },
            new PortLineModel()
            {
                PortNumber = 3,
                TraceTitle =
                    "Very very very long title for quite short trace in Minsk City (testing new MonitoringSettingsView) p3",
                PreciseBaseSpan = TimeSpan.FromSeconds(34),
                FastBaseSpan = TimeSpan.FromSeconds(34),
                IsMonitoringOn = true,
            },
            new PortLineModel()
            {
                PortNumber = 4,
                TraceTitle =
                    "Very very very long title for quite short trace in Minsk City (testing new MonitoringSettingsView) p4",
                PreciseBaseSpan = TimeSpan.FromSeconds(34),
                FastBaseSpan = TimeSpan.FromSeconds(34),
                IsMonitoringOn = true,
            },
                new PortLineModel()
            {
                PortNumber = 1,
                TraceTitle =
                    "Very very very long title for quite short trace in Minsk City (testing new MonitoringSettingsView) p1",
                PreciseBaseSpan = TimeSpan.FromSeconds(73),
                FastBaseSpan = TimeSpan.FromSeconds(34),
                IsMonitoringOn = true,
            },
            new PortLineModel()
            {
                PortNumber = 2,
                TraceTitle =
                    "Very very very long title for quite short trace in Minsk City (testing new MonitoringSettingsView) p2",
                PreciseBaseSpan = TimeSpan.FromSeconds(53),
                FastBaseSpan = TimeSpan.FromSeconds(32),
                IsMonitoringOn = false,
            },
            new PortLineModel()
            {
                PortNumber = 3,
                TraceTitle =
                    "Very very very long title for quite short trace in Minsk City (testing new MonitoringSettingsView) p3",
                PreciseBaseSpan = TimeSpan.FromSeconds(34),
                FastBaseSpan = TimeSpan.FromSeconds(34),
                IsMonitoringOn = true,
            },
            new PortLineModel()
            {
                PortNumber = 4,
                TraceTitle =
                    "Very very very long title for quite short trace in Minsk City (testing new MonitoringSettingsView) p4",
                PreciseBaseSpan = TimeSpan.FromSeconds(34),
                FastBaseSpan = TimeSpan.FromSeconds(34),
                IsMonitoringOn = true,
            } ,
                new PortLineModel()
            {
                PortNumber = 1,
                TraceTitle =
                    "Very very very long title for quite short trace in Minsk City (testing new MonitoringSettingsView) p1",
                PreciseBaseSpan = TimeSpan.FromSeconds(73),
                FastBaseSpan = TimeSpan.FromSeconds(34),
                IsMonitoringOn = true,
            },
            new PortLineModel()
            {
                PortNumber = 2,
                TraceTitle =
                    "Very very very long title for quite short trace in Minsk City (testing new MonitoringSettingsView) p2",
                PreciseBaseSpan = TimeSpan.FromSeconds(53),
                FastBaseSpan = TimeSpan.FromSeconds(32),
                IsMonitoringOn = false,
            },
            new PortLineModel()
            {
                PortNumber = 3,
                TraceTitle =
                    "Very very very long title for quite short trace in Minsk City (testing new MonitoringSettingsView) p3",
                PreciseBaseSpan = TimeSpan.FromSeconds(34),
                FastBaseSpan = TimeSpan.FromSeconds(34),
                IsMonitoringOn = true,
            },
            new PortLineModel()
            {
                PortNumber = 4,
                TraceTitle =
                    "Very very very long title for quite short trace in Minsk City (testing new MonitoringSettingsView) p4",
                PreciseBaseSpan = TimeSpan.FromSeconds(34),
                FastBaseSpan = TimeSpan.FromSeconds(34),
                IsMonitoringOn = true,
            }};
            CycleFullTime = Ports.Sum(p => p.FastBaseSpan.Seconds).ToString();
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Monitoring settings";
        }
    }
}
