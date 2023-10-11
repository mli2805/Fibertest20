using System.Collections.Generic;
using System.Linq;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;
using Utils471;

namespace Iit.Fibertest.DataCenterCore
{
    public class SnmpNotifier
    {
        private readonly IniFile _iniFile;
        private readonly IMyLog _logFile;
        private readonly Model _writeModel;
        private readonly SnmpAgent _snmpAgent;

        public SnmpNotifier(IniFile iniFile, IMyLog logFile, Model writeModel, SnmpAgent snmpAgent)
        {
            _iniFile = iniFile;
            _logFile = logFile;
            _writeModel = writeModel;
            _snmpAgent = snmpAgent;
        }

        public void SendTraceEvent(AddMeasurement meas)
        {
            var isSnmpOn = _iniFile.Read(IniSection.Snmp, IniKey.IsSnmpOn, false);
            if (!isSnmpOn) return;
            var data = MeasToSnmp(meas);

            _snmpAgent.SendRealTrap(data, FtTrapType.MeasurementAsSnmp);
            _logFile.AppendLine("SNMP trap sent");
        }

        public void SendRtuNetworkEvent(NetworkEvent rtuEvent)
        {
            var isSnmpOn = _iniFile.Read(IniSection.Snmp, IniKey.IsSnmpOn, false);
            if (!isSnmpOn) return;
            var data = RtuEventToSnmp(rtuEvent);

            _snmpAgent.SendRealTrap(data, FtTrapType.RtuNetworkEventAsSnmp);
        }

        public void SendBopNetworkEvent(AddBopNetworkEvent bopEvent)
        {
            var isSnmpOn = _iniFile.Read(IniSection.Snmp, IniKey.IsSnmpOn, false);
            if (!isSnmpOn) return;
            var data = BopEventToSnmp(bopEvent);

            _snmpAgent.SendRealTrap(data, FtTrapType.BopNetworkEventAsSnmp);
        }

        public void SendRtuStatusEvent(RtuAccident rtuAccident)
        {
            var isSnmpOn = _iniFile.Read(IniSection.Snmp, IniKey.IsSnmpOn, false);
            if (!isSnmpOn) return;
            var data = RtuStatusEventToSnmp(rtuAccident);

            _snmpAgent.SendRealTrap(data, FtTrapType.RtuStatusEventAsSnmp);
        }

        private List<KeyValuePair<FtTrapProperty, string>> MeasToSnmp(AddMeasurement meas)
        {
            var rtuTitle = _writeModel.Rtus.FirstOrDefault(r => r.Id == meas.RtuId)?.Title ?? "RTU not found";
            var traceTitle = _writeModel.Traces.FirstOrDefault(t => t.TraceId == meas.TraceId)?.Title ??
                             "Trace not found";

            var data = new List<KeyValuePair<FtTrapProperty, string>>
            {
                new KeyValuePair<FtTrapProperty, string>(FtTrapProperty.EventId, meas.SorFileId.ToString()),
                new KeyValuePair<FtTrapProperty, string>(FtTrapProperty.EventRegistrationTime,
                    meas.EventRegistrationTimestamp.ToString("G")),
                new KeyValuePair<FtTrapProperty, string>(FtTrapProperty.RtuTitle, rtuTitle),
                new KeyValuePair<FtTrapProperty, string>(FtTrapProperty.TraceTitle, traceTitle),
                new KeyValuePair<FtTrapProperty, string>(FtTrapProperty.TraceState,
                  meas.TraceState.ToLocalizedString()),
            };
            foreach (var accident in meas.Accidents)
            {
                data.AddRange(AccidentToSnmp(accident));
            }

            return data;
        }

        private List<KeyValuePair<FtTrapProperty, string>> AccidentToSnmp(AccidentOnTraceV2 accident)
        {
            var accidentType = $"{accident.AccidentSeriousness.ToLocalizedString()} ({accident.OpticalTypeOfAccident.ToLetter()})";
            var data = new List<KeyValuePair<FtTrapProperty, string>>()
            {
                new KeyValuePair<FtTrapProperty, string>(FtTrapProperty.AccidentNodeTitle, accident.AccidentTitle ?? ""),
                new KeyValuePair<FtTrapProperty, string>(FtTrapProperty.AccidentType, accidentType),
                new KeyValuePair<FtTrapProperty, string>(FtTrapProperty.AccidentGps, accident.AccidentCoors.ToString()),
                new KeyValuePair<FtTrapProperty, string>(FtTrapProperty.AccidentToRtuDistanceKm,
                    accident.AccidentToRtuOpticalDistanceKm.ToString("0.000")),
            };
            if (accident.Left != null)
            {
                data.Add(new KeyValuePair<FtTrapProperty, string>(FtTrapProperty.LeftNodeTitle, accident.Left.Title ?? ""));
                data.Add(new KeyValuePair<FtTrapProperty, string>(FtTrapProperty.LeftNodeGps, accident.Left.Coors.ToString()));
                data.Add(new KeyValuePair<FtTrapProperty, string>(
                    FtTrapProperty.LeftNodeToRtuDistanceKm, accident.Left.ToRtuOpticalDistanceKm.ToString("0.000")));
            }
            if (accident.Right != null)
            {
                data.Add(new KeyValuePair<FtTrapProperty, string>(FtTrapProperty.RightNodeTitle, accident.Right.Title ?? ""));
                data.Add(new KeyValuePair<FtTrapProperty, string>(FtTrapProperty.RightNodeGps, accident.Right.Coors.ToString()));
                data.Add(new KeyValuePair<FtTrapProperty, string>(
                    FtTrapProperty.RightNodeToRtuDistanceKm, accident.Right.ToRtuOpticalDistanceKm.ToString("0.000")));
            }

            return data;
        }


        private List<KeyValuePair<FtTrapProperty, string>> RtuEventToSnmp(NetworkEvent rtuEvent)
        {
            var rtuTitle = _writeModel.Rtus.FirstOrDefault(r => r.Id == rtuEvent.RtuId)?.Title ?? "RTU not found";

            var data = new List<KeyValuePair<FtTrapProperty, string>>
            {
                new KeyValuePair<FtTrapProperty, string>(FtTrapProperty.EventRegistrationTime,
                    rtuEvent.EventTimestamp.ToString("G")),
                new KeyValuePair<FtTrapProperty, string>(FtTrapProperty.RtuTitle, rtuTitle),
            };
            if (rtuEvent.OnMainChannel != ChannelEvent.Nothing)
                data.Add(
                    new KeyValuePair<FtTrapProperty, string>(FtTrapProperty.RtuMainChannel,
                        rtuEvent.OnMainChannel == ChannelEvent.Repaired ? Resources.SID_Recovered : Resources.SID_Broken));
            if (rtuEvent.OnReserveChannel != ChannelEvent.Nothing)
                data.Add(
                    new KeyValuePair<FtTrapProperty, string>(FtTrapProperty.RtuReserveChannel,
                        rtuEvent.OnReserveChannel == ChannelEvent.Repaired ? Resources.SID_Recovered : Resources.SID_Broken));

            return data;
        }

        private List<KeyValuePair<FtTrapProperty, string>> BopEventToSnmp(AddBopNetworkEvent bopEvent)
        {
            var rtuTitle = _writeModel.Rtus.FirstOrDefault(r => r.Id == bopEvent.RtuId)?.Title ?? "RTU not found";
            var bopTitle =
                _writeModel.Otaus.FirstOrDefault(o => o.NetAddress.Ip4Address == bopEvent.OtauIp)?.NetAddress
                    .ToStringA() ?? "BOP not found";

            var data = new List<KeyValuePair<FtTrapProperty, string>>
            {
                new KeyValuePair<FtTrapProperty, string>(FtTrapProperty.EventRegistrationTime,
                    bopEvent.EventTimestamp.ToString("G")),
                new KeyValuePair<FtTrapProperty, string>(FtTrapProperty.RtuTitle, rtuTitle),
                new KeyValuePair<FtTrapProperty, string>(FtTrapProperty.BopTitle, bopTitle),
                new KeyValuePair<FtTrapProperty, string>(FtTrapProperty.BopState, bopEvent.IsOk
                    ? Resources.SID_Recovered : Resources.SID_Broken),
            };

            return data;
        }

        private List<KeyValuePair<FtTrapProperty, string>> RtuStatusEventToSnmp(RtuAccident rtuStatusEvent)
        {
            var rtuTitle = _writeModel.Rtus.FirstOrDefault(r => r.Id == rtuStatusEvent.RtuId)?.Title ?? "RTU not found";

            var data = new List<KeyValuePair<FtTrapProperty, string>>
            {
                new KeyValuePair<FtTrapProperty, string>(FtTrapProperty.EventRegistrationTime, 
                    rtuStatusEvent.EventRegistrationTimestamp.ToString("G")),
                new KeyValuePair<FtTrapProperty, string>(FtTrapProperty.RtuTitle, rtuTitle),
            };

            if (rtuStatusEvent.IsMeasurementProblem)
            {
                var traceTitle = _writeModel.Traces.FirstOrDefault(t => t.TraceId == rtuStatusEvent.TraceId)?.Title ??
                                 "Trace not found";
                data.Add(new KeyValuePair<FtTrapProperty, string>(FtTrapProperty.TraceTitle, traceTitle));
                data.Add(new KeyValuePair<FtTrapProperty, string>(FtTrapProperty.BaseRefType, rtuStatusEvent.BaseRefType.ToString()));
                data.Add(new KeyValuePair<FtTrapProperty, string>(FtTrapProperty.RtuStatusEventType, "Measurement"));
                data.Add(new KeyValuePair<FtTrapProperty, string>(FtTrapProperty.RtuStatusEventName, rtuStatusEvent.ReturnCode.ToString()));
            }

            return data;
        }
    }
}
