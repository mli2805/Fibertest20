using System;
using System.Collections.Generic;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class EventToLogLineParser
    {
        // ID - Title
        private Dictionary<Guid, string> _rtuTitles;

        // ID - <Title, RTU-ID>
        private Dictionary<Guid, Tuple<string, Guid>> _traces;

        // SorfileId - Measurement
        private Dictionary<int, MeasurementAdded> _measurements;


        public LogLine ParseEventBody(object body)
        {
            switch (body)
            {
                case RtuAtGpsLocationAdded evnt: return Parse(evnt);
                case RtuUpdated evnt: return Parse(evnt);
                case RtuInitialized evnt: return Parse(evnt);
                case RtuRemoved evnt: return Parse(evnt);

                case TraceAdded evnt: return Parse(evnt);
                case TraceUpdated evnt: return Parse(evnt);
                case TraceAttached evnt: return Parse(evnt);
                case TraceDetached evnt: return Parse(evnt);
                case TraceCleaned evnt: return Parse(evnt);
                case TraceRemoved evnt: return Parse(evnt);
                case BaseRefAssigned evnt: return Parse(evnt);

                case MonitoringSettingsChanged evnt: return Parse(evnt);
                case MonitoringStarted evnt: return Parse(evnt);
                case MonitoringStopped evnt: return Parse(evnt);

                case ClientStationRegistered evnt: return Parse(evnt);
                case ClientStationUnregistered _: return new LogLine() { OperationCode = LogOperationCode.ClientExited };
                case ClientConnectionLost _:
                    return new LogLine() { OperationCode = LogOperationCode.ClientConnectionLost };

                case MeasurementAdded evnt: return Parse(evnt);
                case MeasurementUpdated evnt: return Parse(evnt);

                case DbOptimazationStarted evnt: return Parse(evnt);
                default: return null;
            }
        }

        public void Initialize()
        {
            _rtuTitles = new Dictionary<Guid, string>();
            _traces = new Dictionary<Guid, Tuple<string, Guid>>();
            _measurements = new Dictionary<int, MeasurementAdded>();
        }

        private LogLine Parse(RtuAtGpsLocationAdded e)
        {
            _rtuTitles.Add(e.Id, e.Title);
            return new LogLine() { OperationCode = LogOperationCode.RtuAdded, RtuTitle = e.Title };
        }

        private LogLine Parse(RtuUpdated e)
        {
            _rtuTitles[e.RtuId] = e.Title;
            return new LogLine { OperationCode = LogOperationCode.RtuUpdated, RtuTitle = e.Title };
        }

        private LogLine Parse(RtuInitialized e)
        {
            return new LogLine { OperationCode = LogOperationCode.RtuInitialized, RtuTitle = _rtuTitles[e.Id] };
        }

        private LogLine Parse(RtuRemoved e)
        {
            return new LogLine { OperationCode = LogOperationCode.RtuRemoved, RtuTitle = _rtuTitles[e.RtuId] };
        }

        private LogLine Parse(TraceAdded e)
        {
            _traces.Add(e.TraceId, new Tuple<string, Guid>(e.Title, e.RtuId));
            return new LogLine()
            {
                OperationCode = LogOperationCode.TraceAdded,
                RtuTitle = _rtuTitles[e.RtuId],
                TraceTitle = e.Title
            };
        }

        private LogLine Parse(TraceUpdated e)
        {
            var rtuId = _traces[e.Id].Item2;
            _traces[e.Id] = new Tuple<string, Guid>(e.Title, rtuId);
            return new LogLine
            {
                OperationCode = LogOperationCode.TraceUpdated,
                RtuTitle = _rtuTitles[_traces[e.Id].Item2],
                TraceTitle = e.Title,
            };
        }

        private LogLine Parse(TraceAttached e)
        {
            return new LogLine
            {
                OperationCode = LogOperationCode.TraceAttached,
                RtuTitle = _rtuTitles[_traces[e.TraceId].Item2],
                TraceTitle = _traces[e.TraceId].Item1,
                OperationParams = $@"port {e.OtauPortDto.OpticalPort}",
            };
        }

        private LogLine Parse(TraceDetached e)
        {
            return new LogLine
            {
                OperationCode = LogOperationCode.TraceDetached,
                RtuTitle = _rtuTitles[_traces[e.TraceId].Item2],
                TraceTitle = _traces[e.TraceId].Item1,
            };
        }

        private LogLine Parse(TraceCleaned e)
        {
            return new LogLine
            {
                OperationCode = LogOperationCode.TraceCleaned,
                RtuTitle = _rtuTitles[_traces[e.TraceId].Item2],
                TraceTitle = _traces[e.TraceId].Item1,
            };
        }

        private LogLine Parse(TraceRemoved e)
        {
            return new LogLine
            {
                OperationCode = LogOperationCode.TraceRemoved,
                RtuTitle = _rtuTitles[_traces[e.TraceId].Item2],
                TraceTitle = _traces[e.TraceId].Item1,
            };
        }

        private LogLine Parse(BaseRefAssigned e)
        {
            var additionalInfo = "";
            foreach (var baseRef in e.BaseRefs)
            {
                additionalInfo = additionalInfo + baseRef.BaseRefType.GetLocalizedFemaleString() + @"; ";
            }

            return new LogLine()
            {
                OperationCode = LogOperationCode.BaseRefAssigned,
                RtuTitle = _rtuTitles[_traces[e.TraceId].Item2],
                TraceTitle = _traces[e.TraceId].Item1,
                OperationParams = additionalInfo,
            };
        }

        private LogLine Parse(MonitoringSettingsChanged e)
        {
            var mode = e.IsMonitoringOn ? @"AUTO" : @"MANUAL";
            return new LogLine()
            {
                OperationCode = LogOperationCode.MonitoringSettingsChanged,
                RtuTitle = _rtuTitles[e.RtuId],
                OperationParams = $@"Mode - {mode}",
            };
        }

        private LogLine Parse(MonitoringStarted e)
        {
            return new LogLine() { OperationCode = LogOperationCode.MonitoringStarted, RtuTitle = _rtuTitles[e.RtuId] };
        }

        private LogLine Parse(MonitoringStopped e)
        {
            return new LogLine() { OperationCode = LogOperationCode.MonitoringStopped, RtuTitle = _rtuTitles[e.RtuId] };
        }

        private LogLine Parse(ClientStationRegistered e)
        {
            return new LogLine()
            {
                OperationCode = LogOperationCode.ClientStarted,
                OperationParams = e.RegistrationResult.GetLocalizedString(),
            };
        }

        private LogLine Parse(MeasurementAdded e)
        {
            _measurements.Add(e.SorFileId, e);
            return null;
        }

        private LogLine Parse(MeasurementUpdated e)
        {
            var meas = _measurements[e.SorFileId];
            return new LogLine()
            {
                OperationCode = LogOperationCode.MeasurementUpdated,
                RtuTitle = _rtuTitles[meas.RtuId],
                TraceTitle = _traces[meas.TraceId].Item1,
                OperationParams = $@"{e.EventStatus.GetLocalizedString()}",
            };
        }

       
        private LogLine Parse(DbOptimazationStarted e)
        {
            return new LogLine()
            {
                OperationCode = LogOperationCode.DbOptimizationStarted,
                OperationParams =  $@"Up to {e.UpTo}  {e.IsMeasurementsNotEvents}/{e.IsOpticalEvents}/{e.IsNetworkEvents}",
            };
        }
    }
}