using System.Collections.Generic;
using System.Linq;
using Iit.Fibertest.Graph;
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

        private List<KeyValuePair<SnmpProperty, string>> MeasToSnmp(AddMeasurement meas)
        {
            var rtuTitle = _writeModel.Rtus.FirstOrDefault(r => r.Id == meas.RtuId)?.Title ?? "RTU not found";
            var traceTitle = _writeModel.Traces.FirstOrDefault(t => t.TraceId == meas.TraceId)?.Title ??
                             "Trace not found";
          
            var data = new List<KeyValuePair<SnmpProperty, string>>
            {
                new KeyValuePair<SnmpProperty, string>(SnmpProperty.EventRegistrationTime,
                    meas.EventRegistrationTimestamp.ToString("G")),
              new KeyValuePair<SnmpProperty, string>(SnmpProperty.RtuTitle, rtuTitle),
              new KeyValuePair<SnmpProperty, string>(SnmpProperty.TraceTitle, traceTitle),
              new KeyValuePair<SnmpProperty, string>(SnmpProperty.TraceState, 
                  meas.TraceState.ToLocalizedString()),
            };

            return data;
        }

    }
}
