using System.Linq;
using AutoMapper;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Graph
{
 
    public class EchoEventsOnModelExecutor
    {
        private static readonly IMapper Mapper = new MapperConfiguration(
            cfg => cfg.AddProfile<MappingEventToDomainModelProfile>()).CreateMapper();
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
            var rtu =  _model.Rtus.First(r => r.Id == e.Id);
            if (rtu == null)
            {
                var message = $@"RtuInitialized: RTU {e.Id.First6()} not found";
                _logFile.AppendLine(message);
                return message;
            }

            InitializeRtuFirstTime(e, rtu);

            if (rtu.Serial == null)
            {
                return null;
            }

            if (rtu.Serial == e.Serial)
            {
                if (rtu.OwnPortCount != e.OwnPortCount)
                {
                    // main otdr problem
                    // TODO
                    return null;
                }

                if (rtu.FullPortCount != e.FullPortCount)
                {
                    // bop changes
                    // TODO
                    return null;
                }

                if (rtu.FullPortCount == e.FullPortCount)
                {
                    // just re-initialization, nothing should be done?
                    rtu.Version = e.Version;
                }
            }

            if (rtu.Serial != e.Serial)
            {
                //TODO discuss and implement rtu replacement scenario
            }

            return null;
        }

        private void InitializeRtuFirstTime(RtuInitialized e, Rtu rtu)
        {
            rtu.OwnPortCount = e.OwnPortCount;
            rtu.FullPortCount = e.FullPortCount;
            rtu.Serial = e.Serial;
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

            if (e.Otaus != null)
                foreach (var otauAttached in e.Otaus)
                {
                    Otau otau = Mapper.Map<Otau>(otauAttached);
                    otau.NetAddressState = RtuPartState.Ok;
                    _model.Otaus.Add(otau);
                }
        }

        public string ChangeMonitoringSettings(MonitoringSettingsChanged e)
        {
            var rtu =  _model.Rtus.FirstOrDefault(r => r.Id == e.RtuId);
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
            var rtu =  _model.Rtus.FirstOrDefault(r => r.Id == e.RtuId);
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
            var rtu =  _model.Rtus.FirstOrDefault(r => r.Id == e.RtuId);
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