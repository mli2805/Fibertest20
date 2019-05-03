using System;
using System.Linq;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Graph
{
    public static class EchoEventsOnModelExecutor
    {
       
        public static string AssignBaseRef(this Model model, BaseRefAssigned e)
        {
            var trace = model.Traces.FirstOrDefault(t => t.TraceId == e.TraceId);
            if (trace == null)
            {
                return $@"BaseRefAssigned: Trace {e.TraceId} not found";
            }

            var preciseBaseRef = e.BaseRefs.FirstOrDefault(b => b.BaseRefType == BaseRefType.Precise);
            if (preciseBaseRef != null)
            {
                var oldBaseRef = model.BaseRefs.FirstOrDefault(b =>
                    b.TraceId == preciseBaseRef.TraceId && b.BaseRefType == preciseBaseRef.BaseRefType);
                if (oldBaseRef != null)
                    model.BaseRefs.Remove(oldBaseRef);
                if (preciseBaseRef.Id != Guid.Empty)
                    model.BaseRefs.Add(preciseBaseRef);
                trace.PreciseId = preciseBaseRef.Id;
                trace.PreciseDuration = preciseBaseRef.Duration;
            }
            var fastBaseRef = e.BaseRefs.FirstOrDefault(b => b.BaseRefType == BaseRefType.Fast);
            if (fastBaseRef != null)
            {
                var oldBaseRef = model.BaseRefs.FirstOrDefault(b =>
                    b.TraceId == fastBaseRef.TraceId && b.BaseRefType == fastBaseRef.BaseRefType);
                if (oldBaseRef != null)
                    model.BaseRefs.Remove(oldBaseRef);
                if (fastBaseRef.Id != Guid.Empty)
                    model.BaseRefs.Add(fastBaseRef);
                trace.FastId = fastBaseRef.Id;
                trace.FastDuration = fastBaseRef.Duration;
            }
            var additionalBaseRef = e.BaseRefs.FirstOrDefault(b => b.BaseRefType == BaseRefType.Additional);
            if (additionalBaseRef != null)
            {
                var oldBaseRef = model.BaseRefs.FirstOrDefault(b =>
                    b.TraceId == additionalBaseRef.TraceId && b.BaseRefType == additionalBaseRef.BaseRefType);
                if (oldBaseRef != null)
                    model.BaseRefs.Remove(oldBaseRef);
                if (additionalBaseRef.Id != Guid.Empty)
                    model.BaseRefs.Add(additionalBaseRef);
                trace.AdditionalId = additionalBaseRef.Id;
                trace.AdditionalDuration = additionalBaseRef.Duration;
            }
            if (!trace.HasEnoughBaseRefsToPerformMonitoring)
                trace.IsIncludedInMonitoringCycle = false;

            return null;
        }

        public static string InitializeRtu(this Model model, RtuInitialized e)
        {
            var rtu = model.Rtus.First(r => r.Id == e.Id);
            if (rtu == null)
            {
                return $@"RtuInitialized: RTU {e.Id.First6()} not found";
            }

            model.SetRtuProperties(rtu, e);
            return null;
        }

        private static void SetRtuProperties(this Model model, Rtu rtu, RtuInitialized e)
        {
            rtu.OwnPortCount = e.OwnPortCount;
            rtu.FullPortCount = e.FullPortCount;

            if (rtu.Serial != e.Serial)
            {
                foreach (var trace in model.Traces.Where(t => t.OtauPort != null && t.OtauPort.Serial == rtu.Serial))
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

            /*
                    RTU cannot return child OTAU which does not exist yet! It's a business rule
                    Client sends existing OTAU list -> 
                    RTU MUST detach any OTAU which are not in client's list
                    and attach all OTAU from this list
            */
            foreach (var childPair in e.Children)
            {
                var otau = model.Otaus.First(o => o.NetAddress.Equals(childPair.Value.NetAddress));
                if (otau != null)
                    otau.IsOk = childPair.Value.IsOk;
            }
        }

        public static string ChangeMonitoringSettings(this Model model, MonitoringSettingsChanged e)
        {
            var rtu = model.Rtus.FirstOrDefault(r => r.Id == e.RtuId);
            if (rtu == null)
            {
                return $@"MonitoringSettingsChanged: RTU {e.RtuId.First6()} not found";
            }
            rtu.PreciseMeas = e.PreciseMeas;
            rtu.PreciseSave = e.PreciseSave;
            rtu.FastSave = e.FastSave;
            rtu.MonitoringState = e.IsMonitoringOn ? MonitoringState.On : MonitoringState.Off;

            foreach (var trace in model.Traces.Where(t => t.RtuId == e.RtuId))
            {
                trace.IsIncludedInMonitoringCycle = e.TracesInMonitoringCycle.Contains(trace.TraceId);
            }
            return null;
        }

        public static string StartMonitoring(this Model model, MonitoringStarted e)
        {
            var rtu = model.Rtus.FirstOrDefault(r => r.Id == e.RtuId);
            if (rtu == null)
            {
                return $@"MonitoringStarted: RTU {e.RtuId.First6()} not found";
            }
            rtu.MonitoringState = MonitoringState.On;
            return null;
        }

        public static string StopMonitoring(this Model model, MonitoringStopped e)
        {
            var rtu = model.Rtus.FirstOrDefault(r => r.Id == e.RtuId);
            if (rtu == null)
            {
                return $@"MonitoringStopped: RTU {e.RtuId.First6()} not found";
            }
            rtu.MonitoringState = MonitoringState.Off;
            return null;
        }


    }
}