using System.Linq;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Graph
{
    public class EchoEventsOnModelExecutor
    {
        private readonly IMyLog _logFile;
        private readonly Model _model;

        public EchoEventsOnModelExecutor(Model model, IMyLog logFile)
        {
            _model = model;
            _logFile = logFile;
        }

        public string AssignBaseRef(BaseRefAssigned e)
        {
            var trace = _model.Traces.FirstOrDefault(t => t.TraceId == e.TraceId);
            if (trace == null)
            {
                var message = $@"BaseRefAssigned: Trace {e.TraceId} not found";
                _logFile.AppendLine(message);
                return message;
            }

            var preciseBaseRef = e.BaseRefs.FirstOrDefault(b => b.BaseRefType == BaseRefType.Precise);
            if (preciseBaseRef != null)
            {
                var oldBaseRef = _model.BaseRefs.FirstOrDefault(b =>
                    b.TraceId == preciseBaseRef.TraceId && b.BaseRefType == preciseBaseRef.BaseRefType);
                if (oldBaseRef != null)
                    _model.BaseRefs.Remove(oldBaseRef);
                _model.BaseRefs.Add(preciseBaseRef);
                trace.PreciseId = preciseBaseRef.Id;
                trace.PreciseDuration = preciseBaseRef.Duration;
            }
            var fastBaseRef = e.BaseRefs.FirstOrDefault(b => b.BaseRefType == BaseRefType.Fast);
            if (fastBaseRef != null)
            {
                var oldBaseRef = _model.BaseRefs.FirstOrDefault(b =>
                    b.TraceId == fastBaseRef.TraceId && b.BaseRefType == fastBaseRef.BaseRefType);
                if (oldBaseRef != null)
                    _model.BaseRefs.Remove(oldBaseRef);
                _model.BaseRefs.Add(fastBaseRef);
                trace.FastId = fastBaseRef.Id;
                trace.FastDuration = fastBaseRef.Duration;
            }
            var additionalBaseRef = e.BaseRefs.FirstOrDefault(b => b.BaseRefType == BaseRefType.Additional);
            if (additionalBaseRef != null)
            {
                var oldBaseRef = _model.BaseRefs.FirstOrDefault(b =>
                    b.TraceId == additionalBaseRef.TraceId && b.BaseRefType == additionalBaseRef.BaseRefType);
                if (oldBaseRef != null)
                    _model.BaseRefs.Remove(oldBaseRef);
                _model.BaseRefs.Add(additionalBaseRef);
                trace.AdditionalId = additionalBaseRef.Id;
                trace.AdditionalDuration = additionalBaseRef.Duration;
            }
            if (!trace.HasEnoughBaseRefsToPerformMonitoring)
                trace.IsIncludedInMonitoringCycle = false;

            return null;
        }

        public string InitializeRtu(RtuInitialized e)
        {
            var rtu = _model.Rtus.First(r => r.Id == e.Id);
            if (rtu == null)
            {
                var message = $@"RtuInitialized: RTU {e.Id.First6()} not found";
                _logFile.AppendLine(message);
                return message;
            }

            SetRtuProperties(rtu, e);
            return null;
        }

        private void SetRtuProperties(Rtu rtu, RtuInitialized e)
        {
            rtu.OwnPortCount = e.OwnPortCount;
            rtu.FullPortCount = e.FullPortCount;

            if (rtu.Serial != e.Serial)
            {
                foreach (var trace in _model.Traces.Where(t => t.OtauPort != null && t.OtauPort.Serial == rtu.Serial))
                {
                    trace.OtauPort.Serial = e.Serial;
                }
                rtu.Serial = e.Serial;
            }

            rtu.MainChannel = e.MainChannel;
            rtu.MainChannelState = e.MainChannelState;
            rtu.IsReserveChannelSet = e.IsReserveChannelSet;
            if (e.IsReserveChannelSet)
                rtu.ReserveChannel = e.ReserveChannel;
            rtu.ReserveChannelState = e.ReserveChannelState;
            rtu.OtdrNetAddress = e.OtauNetAddress;
            rtu.Version = e.Version;
            rtu.MonitoringState = MonitoringState.Off;
            rtu.AcceptableMeasParams = e.AcceptableMeasParams;

            if (e.Children == null) return;

            foreach (var childPair in e.Children)
            {
                var otau = _model.Otaus.First(o => o.NetAddress.Equals(childPair.Value.NetAddress));
                if (otau == null)
                {
                    _logFile.AppendLine(@"RTU cannot return child OTAU which does not exist yet! It's a business rule");
                    _logFile.AppendLine(@"Client sends existing OTAU list -> ");
                    _logFile.AppendLine(@" RTU MUST detach any OTAU which are not in client's list");
                    _logFile.AppendLine(@" and attach all OTAU from this list");
                }
                else
                    otau.IsOk = childPair.Value.IsOk;
            }
        }

        public string ChangeMonitoringSettings(MonitoringSettingsChanged e)
        {
            var rtu = _model.Rtus.FirstOrDefault(r => r.Id == e.RtuId);
            if (rtu == null)
            {
                var message = $@"MonitoringSettingsChanged: RTU {e.RtuId.First6()} not found";
                _logFile.AppendLine(message);
                return message;
            }
            rtu.PreciseMeas = e.PreciseMeas;
            rtu.PreciseSave = e.PreciseSave;
            rtu.FastSave = e.FastSave;
            rtu.MonitoringState = e.IsMonitoringOn ? MonitoringState.On : MonitoringState.Off;

            foreach (var trace in _model.Traces.Where(t => t.RtuId == e.RtuId))
            {
                trace.IsIncludedInMonitoringCycle = e.TracesInMonitoringCycle.Contains(trace.TraceId);
            }
            return null;
        }

        public string StartMonitoring(MonitoringStarted e)
        {
            var rtu = _model.Rtus.FirstOrDefault(r => r.Id == e.RtuId);
            if (rtu == null)
            {
                var message = $@"MonitoringStarted: RTU {e.RtuId.First6()} not found";
                _logFile.AppendLine(message);
                return message;
            }
            rtu.MonitoringState = MonitoringState.On;
            return null;
        }

        public string StopMonitoring(MonitoringStopped e)
        {
            var rtu = _model.Rtus.FirstOrDefault(r => r.Id == e.RtuId);
            if (rtu == null)
            {
                var message = $@"MonitoringStopped: RTU {e.RtuId.First6()} not found";
                _logFile.AppendLine(message);
                return message;
            }
            rtu.MonitoringState = MonitoringState.Off;
            return null;
        }


    }
}