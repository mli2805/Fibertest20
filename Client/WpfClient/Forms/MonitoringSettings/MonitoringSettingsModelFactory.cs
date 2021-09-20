using System.Collections.Generic;
using System.Linq;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client.MonitoringSettings
{
    public class MonitoringSettingsModelFactory
    {
        private RtuLeaf _rtuLeaf;
        private readonly Model _readModel;
        private readonly CurrentUser _currentUser;

        public MonitoringSettingsModelFactory(Model readModel, CurrentUser currentUser)
        {
            _readModel = readModel;
            _currentUser = currentUser;
        }

        public MonitoringSettingsModel Create(RtuLeaf rtuLeaf, bool isEditEnabled)
        {
            _rtuLeaf = rtuLeaf;
            var rtu = _readModel.Rtus.FirstOrDefault(r => r.Id == _rtuLeaf.Id);
            if (rtu == null)
                return null;
            var model = new MonitoringSettingsModel()
            {
                RtuId = _rtuLeaf.Id,
                RtuMaker = _rtuLeaf.RtuMaker,
                RealOtdrAddress = GetRealOtdrAddress(),
                IsMonitoringOn = _rtuLeaf.MonitoringState == MonitoringState.On,
                Charons = PrepareMonitoringCharonModels(isEditEnabled),
            };
            model.Frequencies.InitializeComboboxes(rtu.FastSave, rtu.PreciseMeas, rtu.PreciseSave);
            return model;
        }

        private string GetRealOtdrAddress()
        {
            return _readModel.Rtus.FirstOrDefault(r => r.Id == _rtuLeaf.Id)?.OtdrNetAddress.Ip4Address;
        }

        private List<MonitoringCharonModel> PrepareMonitoringCharonModels(bool isEditEnabled)
        {
            var charons = new List<MonitoringCharonModel> {PrepareMainCharonModel(isEditEnabled)};
            foreach (var leaf in _rtuLeaf.ChildrenImpresario.Children)
            {
                var otauLeaf = leaf as OtauLeaf;
                if (otauLeaf != null)
                {
                    charons.Add(PrepareBopCharonModel(otauLeaf, isEditEnabled));
                }
            }
            return charons;
        }

        private MonitoringCharonModel PrepareMainCharonModel(bool isEditEnabled)
        {
            var rtu = _readModel.Rtus.First(r => r.Id == _rtuLeaf.Id);
            var mainCharonModel = new MonitoringCharonModel(_rtuLeaf.Serial)
            {
                Title = _rtuLeaf.Title,
                IsMainCharon = true,
                OtauId = rtu.OtauId,
                Ports = PrepareMonitoringPortModels(_rtuLeaf),
                IsEditEnabled = isEditEnabled,
            };
            return mainCharonModel;
        }

        private MonitoringCharonModel PrepareBopCharonModel(OtauLeaf otauLeaf, bool isEditEnabled)
        {
            var otau = _readModel.Otaus.First(o => o.Id == otauLeaf.Id);
            var bopCharonModel = new MonitoringCharonModel(otauLeaf.Serial)
            {
                Title = otauLeaf.OtauNetAddress.ToStringA(),
                IsMainCharon = false,
                OtauId = otau.Id.ToString(),
                MainCharonPort = otau.MasterPort,
                Ports = PrepareMonitoringPortModels(otauLeaf),
                IsEditEnabled = isEditEnabled,
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
                    var trace = _readModel.Traces.FirstOrDefault(t => t.TraceId == traceLeaf.Id);
                    if (trace == null)
                        return null; // it couldn't be!
                    result.Add(new MonitoringPortModel()
                    {
                        PortNumber = i+1,
                        TraceId = traceLeaf.Id,
                        TraceTitle = traceLeaf.Title,
                        PreciseBaseSpan = trace.PreciseDuration,
                        FastBaseSpan = trace.FastDuration,
                        AdditionalBaseSpan = trace.AdditionalDuration,
                        IsIncluded = trace.IsIncludedInMonitoringCycle,
                        IsInCurrentUserZone = trace.ZoneIds.Contains(_currentUser.ZoneId),
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
