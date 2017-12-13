using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class RtuStateVmFactory
    {
        private readonly ReadModel _readModel;
        private readonly OpticalEventsViewModel _opticalEventsViewModel;

        public RtuStateVmFactory(ReadModel readModel, OpticalEventsViewModel opticalEventsViewModel)
        {
            _readModel = readModel;
            _opticalEventsViewModel = opticalEventsViewModel;
        }

        public RtuStateVm Create(RtuLeaf rtuLeaf)
        {
            var rtu = _readModel.Rtus.FirstOrDefault(r => r.Id == rtuLeaf.Id);
            if (rtu == null) return null;

            var rtuStateVm = new RtuStateVm();
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
                    : RtuPartState.Normal;

            rtuStateVm.CurrentMeasurementStep = rtuLeaf.MonitoringState == MonitoringState.Off ? Resources.SID_No_measurement : Resources.SID_Unknown;
            return rtuStateVm;
        }

        private List<PortLineVm> PreparePortLines(ObservableCollection<Leaf> leaves, string mainPort, ref int bopCount, ref bool hasBrokenBop, ref int traceCount)
        {
            var result = new List<PortLineVm>();
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

        private PortLineVm PreparePortLine(PortLeaf portLeaf, string mainPort)
        {
            return new PortLineVm()
            {
                Number = mainPort + portLeaf.PortNumber,
                TraceTitle = Resources.SID_None,
                TraceState = FiberState.Unknown,
            };
        }

        private PortLineVm PreparePortLine(TraceLeaf traceLeaf, string mainPort)
        {
            return new PortLineVm()
            {
                Number = mainPort + traceLeaf.PortNumber,
                TraceId = traceLeaf.Id,
                TraceTitle = traceLeaf.Title,
                TraceState = traceLeaf.TraceState,
                Timestamp = _opticalEventsViewModel.Rows.LastOrDefault(o => o.TraceId == traceLeaf.Id)?.EventRegistrationTimestamp,
            };
        }
    }
}