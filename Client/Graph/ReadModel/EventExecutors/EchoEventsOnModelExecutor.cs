using System.Linq;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Graph
{
    public class EchoEventsOnModelExecutor
    {
        private readonly IModel _model;
        private readonly IMyLog _logFile;

        public EchoEventsOnModelExecutor(ReadModel model, IMyLog logFile)
        {
            _model = model;
            _logFile = logFile;
        }
        public void AssignBaseRef(BaseRefAssigned e)
        {
            var trace = _model.Traces.Single(t => t.Id == e.TraceId);

            var preciseBaseRef = e.BaseRefs.FirstOrDefault(b => b.BaseRefType == BaseRefType.Precise);
            if (preciseBaseRef != null)
            {
                trace.PreciseId = preciseBaseRef.Id;
                trace.PreciseDuration = preciseBaseRef.Duration;
            }
            var fastBaseRef = e.BaseRefs.FirstOrDefault(b => b.BaseRefType == BaseRefType.Fast);
            if (fastBaseRef != null)
            {
                trace.FastId = fastBaseRef.Id;
                trace.FastDuration = fastBaseRef.Duration;
            }
            var additionalBaseRef = e.BaseRefs.FirstOrDefault(b => b.BaseRefType == BaseRefType.Additional);
            if (additionalBaseRef != null)
            {
                trace.AdditionalId = additionalBaseRef.Id;
                trace.AdditionalDuration = additionalBaseRef.Duration;
            }
            if (!trace.HasEnoughBaseRefsToPerformMonitoring)
                trace.IsIncludedInMonitoringCycle = false;
        }

        public void InitializeRtu(RtuInitialized e)
        {
            var rtu =  _model.Rtus.First(r => r.Id == e.Id);
            InitializeRtuFirstTime(e, rtu);

            if (rtu.Serial == null)
            {
                return;
            }

            if (rtu.Serial == e.Serial)
            {
                if (rtu.OwnPortCount != e.OwnPortCount)
                {
                    // main otdr problem
                    // TODO
                    return;
                }

                if (rtu.FullPortCount != e.FullPortCount)
                {
                    // bop changes
                    // TODO
                    return;
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
        }

        private static void InitializeRtuFirstTime(RtuInitialized e, Rtu rtu)
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
        }

        public void ChangeMonitoringSettings(MonitoringSettingsChanged e)
        {
            var rtu =  _model.Rtus.FirstOrDefault(r => r.Id == e.RtuId);
            if (rtu == null)
            {
                _logFile.AppendLine(@"MonitoringSettingsChanged: cant find RTU");
                return;
            }
            rtu.PreciseMeas = e.PreciseMeas;
            rtu.PreciseSave = e.PreciseSave;
            rtu.FastSave = e.FastSave;
            rtu.MonitoringState = e.IsMonitoringOn ? MonitoringState.On : MonitoringState.Off;

            foreach (var trace in _model.Traces.Where(t => t.RtuId == e.RtuId))
            {
                trace.IsIncludedInMonitoringCycle = e.TracesInMonitoringCycle.Contains(trace.Id);
            }
        }

        public void StartMonitoring(MonitoringStarted e)
        {
            var rtu =  _model.Rtus.FirstOrDefault(r => r.Id == e.RtuId);
            if (rtu == null)
            {
                _logFile.AppendLine(@"MonitoringStarted: cant find RTU");
                return;
            }
            rtu.MonitoringState = MonitoringState.On;
        }

        public void StopMonitoring(MonitoringStopped e)
        {
            var rtu =  _model.Rtus.FirstOrDefault(r => r.Id == e.RtuId);
            if (rtu == null)
            {
                _logFile.AppendLine(@"MonitoringStopped: cant find RTU");
                return;
            }
            rtu.MonitoringState = MonitoringState.Off;
        }
    }
}