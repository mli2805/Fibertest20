using System;
using System.Collections.Generic;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class EventToLogLineParser
    {
        // ID - Title
        private Dictionary<Guid, string> _rtuTitles;
        // ID - <Title, RTU-ID>
        private Dictionary<Guid, Tuple<string, Guid>> _traces;

        public void Initialize()
        {
            _rtuTitles = new Dictionary<Guid, string>();
            _traces = new Dictionary<Guid, Tuple<string, Guid>>();
        }

        public LogLine Parse(RtuAtGpsLocationAdded e)
        {
            _rtuTitles.Add(e.Id, e.Title);
            return new LogLine() { OperationName = Resources.SID_RTU_added, RtuTitle = e.Title };
        }

        public LogLine Parse(RtuUpdated e)
        {
            _rtuTitles[e.RtuId] = e.Title;
            return new LogLine { OperationName = Resources.SID_RTU_updated, RtuTitle = e.Title };
        }

        public LogLine Parse(RtuInitialized e)
        {
            return new LogLine { OperationName = Resources.SID_RTU_initialized2, RtuTitle = _rtuTitles[e.Id] };
        }

        public LogLine Parse(TraceAdded e)
        {
            _traces.Add(e.TraceId, new Tuple<string, Guid>(e.Title, e.RtuId));
            return new LogLine() { OperationName = Resources.SID_Trace_added, RtuTitle = _rtuTitles[e.RtuId], TraceTitle = e.Title };
        }

        public LogLine Parse(TraceUpdated e)
        {
            var rtuId = _traces[e.Id].Item2;
            _traces[e.Id] = new Tuple<string, Guid>(e.Title, rtuId);
            return new LogLine
            {
                OperationName = Resources.SID_Trace_updated,
                RtuTitle = _rtuTitles[_traces[e.Id].Item2],
                TraceTitle = e.Title,
            };
        }

        public LogLine Parse(TraceAttached e)
        {
            return new LogLine
            {
                OperationName = Resources.SID_Trace_attached,
                RtuTitle = _rtuTitles[_traces[e.TraceId].Item2],
                TraceTitle = _traces[e.TraceId].Item1,
            };
        }
        public LogLine Parse(TraceDetached e)
        {
            return new LogLine
            {
                OperationName = Resources.SID_Trace_detached,
                RtuTitle = _rtuTitles[_traces[e.TraceId].Item2],
                TraceTitle = _traces[e.TraceId].Item1,
            };
        }
        public LogLine Parse(TraceCleaned e)
        {
            return new LogLine
            {
                OperationName = Resources.SID_Trace_cleaned,
                RtuTitle = _rtuTitles[_traces[e.TraceId].Item2],
                TraceTitle = _traces[e.TraceId].Item1,
            };
        }
        public LogLine Parse(TraceRemoved e)
        {
            return new LogLine
            {
                OperationName = Resources.SID_Trace_removed,
                RtuTitle = _rtuTitles[_traces[e.TraceId].Item2],
                TraceTitle = _traces[e.TraceId].Item1,
            };
        }

        public LogLine Parse(BaseRefAssigned e)
        {
            return new LogLine()
            {
                OperationName = Resources.SID_Base_ref_assigned,
                RtuTitle = _rtuTitles[_traces[e.TraceId].Item2],
                TraceTitle = _traces[e.TraceId].Item1,
            };
        }

        public LogLine Parse(MonitoringSettingsChanged e)
        {
            return new LogLine() { OperationName = Resources.SID_Monitoring_settings_changed, RtuTitle = _rtuTitles[e.RtuId] };
        }

        public LogLine Parse(MonitoringStopped e)
        {
            return new LogLine() { OperationName = Resources.SID_Monitoring_stopped, RtuTitle = _rtuTitles[e.RtuId] };
        }

        public LogLine Parse(ClientStationRegistered e) { return new LogLine() { OperationName = Resources.SID_Client_started }; }
        public LogLine Parse(ClientStationUnregistered e) { return new LogLine() { OperationName = Resources.SID_Client_exited }; }
    }
}