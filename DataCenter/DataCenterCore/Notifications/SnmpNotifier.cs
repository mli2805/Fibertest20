using System.Collections.Generic;
using System.Linq;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.DataCenterCore
{
    public class SnmpNotifier
    {
        private readonly IniFile _iniFile;
        private readonly Model _writeModel;
        private readonly SnmpAgent _snmpAgent;

        public SnmpNotifier(IniFile iniFile, Model writeModel, SnmpAgent snmpAgent)
        {
            _iniFile = iniFile;
            _writeModel = writeModel;
            _snmpAgent = snmpAgent;
        }

        public void SendTraceEvent(AddMeasurement meas)
        {
            var isSnmpOn = _iniFile.Read(IniSection.Snmp, IniKey.IsSnmpOn, true);
            if (!isSnmpOn) return;
            var data = MeasToSnmp(meas);

            _snmpAgent.SentRealTrap(data, SnmpTrapType.MeasurementAsSnmp);
        }

        public void SendRtuNetworkEvent(NetworkEvent rtuEvent)
        {
            var isSnmpOn = _iniFile.Read(IniSection.Snmp, IniKey.IsSnmpOn, true);
            if (!isSnmpOn) return;
            var data = RtuEventToSnmp(rtuEvent);

            _snmpAgent.SentRealTrap(data, SnmpTrapType.RtuNetworkEventAsSnmp);
        }

        public void SendBopNetworkEvent(AddBopNetworkEvent bopEvent)
        {
            var isSnmpOn = _iniFile.Read(IniSection.Snmp, IniKey.IsSnmpOn, true);
            if (!isSnmpOn) return;
            var data = BopEventToSnmp(bopEvent);

            _snmpAgent.SentRealTrap(data, SnmpTrapType.BopNetworkEventAsSnmp);

        }

        private List<KeyValuePair<SnmpProperty, string>> MeasToSnmp(AddMeasurement meas)
        {
            var rtuTitle = _writeModel.Rtus.FirstOrDefault(r => r.Id == meas.RtuId)?.Title ?? "RTU not found";
            var traceTitle = _writeModel.Traces.FirstOrDefault(t => t.TraceId == meas.TraceId)?.Title ??
                             "Trace not found";

            var data = new List<KeyValuePair<SnmpProperty, string>>
            {
                new KeyValuePair<SnmpProperty, string>(SnmpProperty.EventId, meas.SorFileId.ToString()),
                new KeyValuePair<SnmpProperty, string>(SnmpProperty.EventRegistrationTime,
                    meas.EventRegistrationTimestamp.ToString("G")),
                new KeyValuePair<SnmpProperty, string>(SnmpProperty.RtuTitle, rtuTitle),
                new KeyValuePair<SnmpProperty, string>(SnmpProperty.TraceTitle, traceTitle),
                new KeyValuePair<SnmpProperty, string>(SnmpProperty.TraceState,
                  meas.TraceState.ToLocalizedString()),
            };
            foreach (var accident in meas.Accidents)
            {
                data.AddRange(AccidentToSnmp(accident));
            }

            return data;
        }

        private List<KeyValuePair<SnmpProperty, string>> AccidentToSnmp(AccidentOnTraceV2 accident)
        {
            var accidentType = $"{accident.AccidentSeriousness.ToLocalizedString()} ({accident.OpticalTypeOfAccident.ToLetter()})";
            var data = new List<KeyValuePair<SnmpProperty, string>>()
            {
                new KeyValuePair<SnmpProperty, string>(SnmpProperty.AccidentNodeTitle, accident.AccidentTitle ?? ""),
                new KeyValuePair<SnmpProperty, string>(SnmpProperty.AccidentType, accidentType),
                new KeyValuePair<SnmpProperty, string>(SnmpProperty.AccidentGps, accident.AccidentCoors.ToString()),
                new KeyValuePair<SnmpProperty, string>(SnmpProperty.AccidentToRtuDistanceKm,
                    accident.AccidentToRtuOpticalDistanceKm.ToString("0.000")),
            };
            if (accident.Left != null)
            {
                data.Add(new KeyValuePair<SnmpProperty, string>(SnmpProperty.LeftNodeTitle, accident.Left.Title ?? ""));
                data.Add(new KeyValuePair<SnmpProperty, string>(SnmpProperty.LeftNodeGps, accident.Left.Coors.ToString()));
                data.Add(new KeyValuePair<SnmpProperty, string>(
                    SnmpProperty.LeftNodeToRtuDistanceKm, accident.Left.ToRtuOpticalDistanceKm.ToString("0.000")));
            }
            if (accident.Right != null)
            {
                data.Add(new KeyValuePair<SnmpProperty, string>(SnmpProperty.RightNodeTitle, accident.Right.Title ?? ""));
                data.Add(new KeyValuePair<SnmpProperty, string>(SnmpProperty.RightNodeGps, accident.Right.Coors.ToString()));
                data.Add(new KeyValuePair<SnmpProperty, string>(
                    SnmpProperty.RightNodeToRtuDistanceKm, accident.Right.ToRtuOpticalDistanceKm.ToString("0.000")));
            }

            return data;
        }


        private List<KeyValuePair<SnmpProperty, string>> RtuEventToSnmp(NetworkEvent rtuEvent)
        {
            var rtuTitle = _writeModel.Rtus.FirstOrDefault(r => r.Id == rtuEvent.RtuId)?.Title ?? "RTU not found";

            var data = new List<KeyValuePair<SnmpProperty, string>>
            {
                new KeyValuePair<SnmpProperty, string>(SnmpProperty.EventRegistrationTime,
                    rtuEvent.EventTimestamp.ToString("G")),
                new KeyValuePair<SnmpProperty, string>(SnmpProperty.RtuTitle, rtuTitle),
            };
            if (rtuEvent.OnMainChannel != ChannelEvent.Nothing)
                data.Add(
                    new KeyValuePair<SnmpProperty, string>(SnmpProperty.RtuMainChannel,
                        rtuEvent.OnMainChannel == ChannelEvent.Repaired ? Resources.SID_Recovered : Resources.SID_Broken));
            if (rtuEvent.OnReserveChannel != ChannelEvent.Nothing)
                data.Add(
                    new KeyValuePair<SnmpProperty, string>(SnmpProperty.RtuReserveChannel,
                        rtuEvent.OnReserveChannel == ChannelEvent.Repaired ? Resources.SID_Recovered : Resources.SID_Broken));

            return data;
        }

        private List<KeyValuePair<SnmpProperty, string>> BopEventToSnmp(AddBopNetworkEvent bopEvent)
        {
            var rtuTitle = _writeModel.Rtus.FirstOrDefault(r => r.Id == bopEvent.RtuId)?.Title ?? "RTU not found";
            var bopTitle =
                _writeModel.Otaus.FirstOrDefault(o => o.OtauAddress.Ip4Address == bopEvent.OtauIp)?.OtauAddress
                    .ToStringA() ?? "BOP not found";

            var data = new List<KeyValuePair<SnmpProperty, string>>
            {
                new KeyValuePair<SnmpProperty, string>(SnmpProperty.EventRegistrationTime,
                    bopEvent.EventTimestamp.ToString("G")),
                new KeyValuePair<SnmpProperty, string>(SnmpProperty.RtuTitle, rtuTitle),
                new KeyValuePair<SnmpProperty, string>(SnmpProperty.BopTitle, bopTitle),
                new KeyValuePair<SnmpProperty, string>(SnmpProperty.BopState, bopEvent.IsOk
                    ? Resources.SID_Recovered : Resources.SID_Broken),
            };

            return data;
        }
    }
}
