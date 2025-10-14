using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Utils471;

namespace Iit.Fibertest.DataCenterCore
{
    public class SnmpDataFactory
    {
        public Dictionary<FtTrapProperty, string> Create(Model writeModel, object e)
        {
            if (!writeModel.SnmpNewSettings.Enabled) return null;

            var prevCulture = Thread.CurrentThread.CurrentUICulture;

            var cultureInfo = new CultureInfo(writeModel.SnmpNewSettings.SnmpLanguage) { NumberFormat = { NumberDecimalSeparator = @"." } };
            Thread.CurrentThread.CurrentCulture = cultureInfo;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;

            Dictionary<FtTrapProperty, string> result = null;
            switch (e)
            {
                case AddMeasurement evnt: result = MeasToSnmp(evnt, writeModel); break;
                case AccidentOnTraceV2 evnt: result = AccidentToSnmp(evnt); break;
                case NetworkEvent evnt : result = RtuEventToSnmp(evnt, writeModel); break;
                case BopNetworkEvent evnt : result = BopEventToSnmp(evnt, writeModel); break;
                case RtuAccident evnt : result = RtuStatusEventToSnmp(evnt, writeModel); break;
            }

            Thread.CurrentThread.CurrentCulture = prevCulture;
            Thread.CurrentThread.CurrentUICulture = prevCulture;

            return result;
        }

        private Dictionary<FtTrapProperty, string> MeasToSnmp(AddMeasurement meas, Model writeModel)
        {
            var rtuTitle = writeModel.Rtus.FirstOrDefault(r => r.Id == meas.RtuId)?.Title ?? "RTU not found";
            var traceTitle = writeModel.Traces.FirstOrDefault(t => t.TraceId == meas.TraceId)?.Title ??
                             "Trace not found";

            var data = new Dictionary<FtTrapProperty, string>
            {
                {FtTrapProperty.EventId, meas.SorFileId.ToString()},
                {FtTrapProperty.EventRegistrationTime, meas.EventRegistrationTimestamp.ToString("G")},
                {FtTrapProperty.RtuTitle, rtuTitle},
                {FtTrapProperty.TraceTitle, traceTitle},
                { FtTrapProperty.TraceState, meas.TraceState.ToLocalizedString()},
            };

            foreach (var accident in meas.Accidents)
            {
                foreach (var pair in AccidentToSnmp(accident))
                {
                    data.Add(pair.Key, pair.Value);
                }
            }

            return data;
        }

        private Dictionary<FtTrapProperty, string> AccidentToSnmp(AccidentOnTraceV2 accident)
        {
            // var accidentType = $"{accident.AccidentSeriousness.ToLocalizedString()} ({accident.OpticalTypeOfAccident.ToLetter()})";
            var accidentType = $"{accident.OpticalTypeOfAccident.ToLetter()}";
            var data = new Dictionary<FtTrapProperty, string>
            {
                {FtTrapProperty.AccidentNodeTitle, accident.AccidentTitle ?? ""},
                {FtTrapProperty.AccidentType, accidentType},
                {FtTrapProperty.AccidentGps, accident.AccidentCoors.ToString()},
                {FtTrapProperty.AccidentToRtuDistanceKm, accident.AccidentToRtuOpticalDistanceKm.ToString("0.000")},
            };
            if (accident.Left != null)
            {
                data.Add(FtTrapProperty.LeftNodeTitle, accident.Left.Title ?? "");
                data.Add(FtTrapProperty.LeftNodeGps, accident.Left.Coors.ToString());
                data.Add(FtTrapProperty.LeftNodeToRtuDistanceKm, accident.Left.ToRtuOpticalDistanceKm.ToString("0.000"));
            }
            if (accident.Right != null)
            {
                data.Add(FtTrapProperty.RightNodeTitle, accident.Right.Title ?? "");
                data.Add(FtTrapProperty.RightNodeGps, accident.Right.Coors.ToString());
                data.Add(FtTrapProperty.RightNodeToRtuDistanceKm, accident.Right.ToRtuOpticalDistanceKm.ToString("0.000"));
            }

            return data;
        }


        private Dictionary<FtTrapProperty, string> RtuEventToSnmp(NetworkEvent rtuEvent, Model writeModel)
        {
            var rtuTitle = writeModel.Rtus.FirstOrDefault(r => r.Id == rtuEvent.RtuId)?.Title ?? "RTU not found";

            var data = new Dictionary<FtTrapProperty, string>
            {
                {FtTrapProperty.EventId, rtuEvent.Ordinal.ToString()},
                {FtTrapProperty.EventRegistrationTime, rtuEvent.EventTimestamp.ToString("G")},
                { FtTrapProperty.RtuTitle, rtuTitle},
            };
            if (rtuEvent.OnMainChannel != ChannelEvent.Nothing)
                data.Add(
                    FtTrapProperty.RtuMainChannel,
                        rtuEvent.OnMainChannel == ChannelEvent.Repaired ? Resources.SID_Recovered : Resources.SID_Broken);
            if (rtuEvent.OnReserveChannel != ChannelEvent.Nothing)
                data.Add(
                    FtTrapProperty.RtuReserveChannel,
                        rtuEvent.OnReserveChannel == ChannelEvent.Repaired ? Resources.SID_Recovered : Resources.SID_Broken);

            return data;
        }

        private Dictionary<FtTrapProperty, string> BopEventToSnmp(BopNetworkEvent bopEvent, Model writeModel)
        {
            var rtuTitle = writeModel.Rtus.FirstOrDefault(r => r.Id == bopEvent.RtuId)?.Title ?? "RTU not found";
            var bopTitle =
                writeModel.Otaus.FirstOrDefault(o => o.NetAddress.Ip4Address == bopEvent.OtauIp)?.NetAddress
                    .ToStringA() ?? "BOP not found";

            var data = new Dictionary<FtTrapProperty, string>
            {
                {FtTrapProperty.EventId, bopEvent.Ordinal.ToString()},
                {FtTrapProperty.EventRegistrationTime, bopEvent.EventTimestamp.ToString("G")},
                {FtTrapProperty.RtuTitle, rtuTitle},
                {FtTrapProperty.BopTitle, bopTitle},
                { FtTrapProperty.BopState, bopEvent.IsOk ? Resources.SID_Recovered : Resources.SID_Broken},
            };

            return data;
        }

        private Dictionary<FtTrapProperty, string> RtuStatusEventToSnmp(RtuAccident rtuStatusEvent, Model writeModel)
        {
            var rtuTitle = writeModel.Rtus.FirstOrDefault(r => r.Id == rtuStatusEvent.RtuId)?.Title ?? "RTU not found";

            var data = new Dictionary<FtTrapProperty, string>
            {
                { FtTrapProperty.EventId, rtuStatusEvent.Id.ToString() },
                {FtTrapProperty.EventRegistrationTime, rtuStatusEvent.EventRegistrationTimestamp.ToString("G")},
                { FtTrapProperty.RtuTitle, rtuTitle},
            };

            if (rtuStatusEvent.IsMeasurementProblem)
            {
                var traceTitle = writeModel.Traces.FirstOrDefault(t => t.TraceId == rtuStatusEvent.TraceId)?.Title ??
                                 "Trace not found";
                data.Add(FtTrapProperty.TraceTitle, traceTitle);
                data.Add(FtTrapProperty.BaseRefType, rtuStatusEvent.BaseRefType.GetLocalizedString());
                data.Add(FtTrapProperty.RtuStatusEventType, rtuStatusEvent.ReturnCode.RtuStatusEventToLocalizedString());

                var explanation = string.Format(rtuStatusEvent.ReturnCode.GetLocalizedString(), rtuStatusEvent.BaseRefType.GetLocalizedFemaleString());
                data.Add(FtTrapProperty.RtuStatusEventName, explanation);
            }

            return data;
        }
    }
}
