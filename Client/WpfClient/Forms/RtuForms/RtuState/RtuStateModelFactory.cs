using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class RtuStateModelFactory
    {
        private readonly ReadModel _readModel;

        public RtuStateModelFactory(ReadModel readModel)
        {
            _readModel = readModel;
        }

        public RtuStateModel Create(RtuLeaf rtuLeaf)
        {
            var rtu = _readModel.Rtus.FirstOrDefault(r => r.Id == rtuLeaf.Id);
            if (rtu == null) return null;

            var rtuStateVm = new RtuStateModel();
            rtuStateVm.Title = rtuLeaf.Title;

            rtuStateVm.MainAddress = rtu.MainChannel.ToStringA();
            rtuStateVm.MainAddressState = rtuLeaf.MainChannelState;

            rtuStateVm.HasReserveAddress = rtu.IsReserveChannelSet;
            rtuStateVm.ReserveAddress = rtu.IsReserveChannelSet ? rtu.ReserveChannel.ToStringB() : Resources.SID_None;
            rtuStateVm.ReserveAddressState = rtuLeaf.ReserveChannelState;

            rtuStateVm.FullPortCount = rtuLeaf.FullPortCount;
            rtuStateVm.OwnPortCount = rtuLeaf.OwnPortCount;

            rtuStateVm.MonitoringMode = rtuLeaf.MonitoringState.ToLocalizedString();

            var bopCount = 0;
            var hasBrokenBop = false;
            var traceCount = 0;
            rtuStateVm.Ports = PreparePortLines(rtuLeaf.ChildrenImpresario.Children, "", ref bopCount, ref hasBrokenBop, ref traceCount);
            rtuStateVm.SetWorstTraceStateAsAggregate();
            rtuStateVm.TraceCount = traceCount;
            rtuStateVm.BopCount = bopCount;
            rtuStateVm.BopState = bopCount == 0
                ? RtuPartState.NotSetYet
                : hasBrokenBop
                    ? RtuPartState.Broken
                    : RtuPartState.Ok;

            rtuStateVm.CurrentMeasurementStep = rtuLeaf.MonitoringState == MonitoringState.Off ? Resources.SID_No_measurement : Resources.SID_Unknown;
            return rtuStateVm;
        }

        private List<PortLineModel> PreparePortLines(ObservableCollection<Leaf> leaves, string mainPort, ref int bopCount, ref bool hasBrokenBop, ref int traceCount)
        {
            var result = new List<PortLineModel>();
            foreach (var leaf in leaves)
            {
                var portLeaf = leaf as PortLeaf;
                if (portLeaf != null)
                    result.Add(PreparePortLine(portLeaf, mainPort));

                var traceLeaf = leaf as TraceLeaf;
                if (traceLeaf != null && traceLeaf.PortNumber > 0)
                {
                    result.Add(PreparePortLine(traceLeaf, mainPort));
                    traceCount++;
                }

                var bopLeaf = leaf as OtauLeaf;
                if (bopLeaf != null)
                {
                    bopCount++;
                    hasBrokenBop = bopLeaf.OtauState == RtuPartState.Broken;
                    result.AddRange(
                        PreparePortLines(bopLeaf.ChildrenImpresario.Children,
                            mainPort + bopLeaf.MasterPort + @"-", ref bopCount, ref hasBrokenBop, ref traceCount));
                }
            }
            return result;
        }

        private PortLineModel PreparePortLine(PortLeaf portLeaf, string mainPort)
        {
            return new PortLineModel()
            {
                Number = mainPort + portLeaf.PortNumber,
                TraceTitle = Resources.SID_None,
                TraceState = FiberState.Unknown,
            };
        }

        private PortLineModel PreparePortLine(TraceLeaf traceLeaf, string mainPort)
        {
            var lastMeasurement = _readModel.Measurements.LastOrDefault(m=>m.TraceId == traceLeaf.Id);
            return new PortLineModel()
            {
                Number = mainPort + traceLeaf.PortNumber,
                TraceId = traceLeaf.Id,
                TraceTitle = traceLeaf.Title,
                TraceState = traceLeaf.TraceState,
                Timestamp = lastMeasurement?.EventRegistrationTimestamp,
                LastSorFileId = lastMeasurement?.SorFileId.ToString(),
            };
        }
    }
}