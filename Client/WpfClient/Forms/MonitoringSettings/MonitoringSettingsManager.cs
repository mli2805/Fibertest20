using System;
using System.Collections.Generic;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client.MonitoringSettings
{
    public class MonitoringSettingsManager
    {
        private readonly RtuLeaf _rtuLeaf;

        public MonitoringSettingsManager(RtuLeaf rtuLeaf)
        {
            _rtuLeaf = rtuLeaf;
        }

        public MonitoringSettingsModel PrepareMonitoringSettingsModel()
        {
            var model = new MonitoringSettingsModel()
            {
                IsMonitoringOn = _rtuLeaf.MonitoringState == MonitoringState.On,
                Charons = PrepareMonitoringCharonModels(),
            };
            model.Frequencies.InitializeComboboxes(Frequency.EveryHour, Frequency.EveryHour, Frequency.EveryHour);
            return model;
        }

        private List<MonitoringCharonModel> PrepareMonitoringCharonModels()
        {
            var charons = new List<MonitoringCharonModel> {PrepareMainCharonModel()};
            foreach (var leaf in _rtuLeaf.ChildrenImpresario.Children)
            {
                var otauLeaf = leaf as OtauLeaf;
                if (otauLeaf != null)
                {
                    charons.Add(PrepareBopCharonModel(otauLeaf));
                }
            }
            return charons;
        }

        private MonitoringCharonModel PrepareMainCharonModel()
        {
            var mainCharonModel = new MonitoringCharonModel(_rtuLeaf.OtauNetAddress.Ip4Address, 23)
            {
                Title = _rtuLeaf.Title,
                IsMainCharon = true,
                Ports = PrepareMonitoringPortModels(_rtuLeaf),
            };
            return mainCharonModel;
        }

        private MonitoringCharonModel PrepareBopCharonModel(OtauLeaf otauLeaf)
        {
            var bopCharonModel = new MonitoringCharonModel(otauLeaf.OtauNetAddress.Ip4Address, otauLeaf.OtauNetAddress.Port)
            {
                Title = otauLeaf.OtauNetAddress.ToStringA(),
                IsMainCharon = false,
                Ports = PrepareMonitoringPortModels(otauLeaf),
            };
            return bopCharonModel;
        }

        private List<MonitoringPortModel> PrepareMonitoringPortModels(IPortOwner portOwner)
        {
            var result = new List<MonitoringPortModel>();
//            var i = 1;
//            foreach (var leaf in portOwner.ChildrenImpresario.Children)
            for (int i = 0; i < portOwner.OwnPortCount; i++)
            {

                var traceLeaf = portOwner.ChildrenImpresario.Children[i] as TraceLeaf;
                if (traceLeaf != null)
                {
                    result.Add(new MonitoringPortModel()
                    {
                        PortNumber = i+1,
                        TraceTitle = traceLeaf.Title,
                    });
                }
                else
                {
                    result.Add(new MonitoringPortModel()
                    {
                        PortNumber = i+1,
                        TraceTitle = "",
                    });

                }
//                i++;
            }
            return result;
        }
    }
}
