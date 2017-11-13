using System.Collections.Generic;
using System.Linq;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client.MonitoringSettings
{
    public class MonitoringSettingsManager
    {
        private readonly RtuLeaf _rtuLeaf;
        private readonly ReadModel _readModel;

        public MonitoringSettingsManager(RtuLeaf rtuLeaf, ReadModel readModel)
        {
            _rtuLeaf = rtuLeaf;
            _readModel = readModel;
        }

        public MonitoringSettingsModel PrepareMonitoringSettingsModel()
        {
            var rtu = _readModel.Rtus.FirstOrDefault(r => r.Id == _rtuLeaf.Id);
            if (rtu == null)
                return null;
            var model = new MonitoringSettingsModel()
            {
                RtuId = _rtuLeaf.Id,
                RealOtdrAddress = GetRealOtdrAddress(),
                IsMonitoringOn = _rtuLeaf.MonitoringState == MonitoringState.On,
                Charons = PrepareMonitoringCharonModels(),
            };
            model.Frequencies.InitializeComboboxes(rtu.FastSave, rtu.PreciseMeas, rtu.PreciseSave);
            return model;
        }

        private string GetRealOtdrAddress()
        {
            return _readModel.Rtus.FirstOrDefault(r => r.Id == _rtuLeaf.Id)?.OtdrNetAddress.Ip4Address;
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
            for (int i = 0; i < portOwner.OwnPortCount; i++)
            {

                var traceLeaf = portOwner.ChildrenImpresario.Children[i] as TraceLeaf;
                if (traceLeaf != null)
                {
                    var trace = _readModel.Traces.FirstOrDefault(t => t.Id == traceLeaf.Id);
                    if (trace == null)
                        return null; // it couldn't be!
                    result.Add(new MonitoringPortModel()
                    {
                        PortNumber = i+1,
                        TraceTitle = traceLeaf.Title,
                        PreciseBaseSpan = trace.PreciseDuration,
                        FastBaseSpan = trace.FastDuration,
                        AdditionalBaseSpan = trace.AdditionalDuration,
                        IsIncluded = trace.IsIncludedInMonitoringCycle,
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
            }
            return result;
        }
    }
}
