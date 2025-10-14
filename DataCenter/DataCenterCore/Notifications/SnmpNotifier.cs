using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;
using Utils471;

namespace Iit.Fibertest.DataCenterCore
{
    public class SnmpNotifier
    {
        private readonly IMyLog _logFile;
        private readonly Model _writeModel;
        private readonly ISnmpService _snmpService;

        public SnmpNotifier(IMyLog logFile, Model writeModel, ISnmpService snmpService)
        {
            _logFile = logFile;
            _writeModel = writeModel;
            _snmpService = snmpService;
        }

        public void SendTraceEvent(AddMeasurement meas)
        {
            var data = new SnmpDataFactory().Create(_writeModel, meas);
            if (data == null) return;
            _snmpService.SendSnmpTrap(_writeModel.SnmpNewSettings, FtTrapType.MeasurementAsSnmp, data);
            _logFile.AppendLine("SNMP trap sent");
        }

        public void SendRtuNetworkEvent(NetworkEvent rtuEvent)
        {
            var data = new SnmpDataFactory().Create(_writeModel, rtuEvent);
            if (data == null) return;
            _snmpService.SendSnmpTrap(_writeModel.SnmpNewSettings, FtTrapType.RtuNetworkEventAsSnmp, data);
        }

        public void SendBopNetworkEvent(BopNetworkEvent bopEvent)
        {
            var data = new SnmpDataFactory().Create(_writeModel, bopEvent);
            if (data == null) return;
            _snmpService.SendSnmpTrap(_writeModel.SnmpNewSettings, FtTrapType.BopNetworkEventAsSnmp, data);
        }

        public void SendRtuStatusEvent(RtuAccident rtuAccident)
        {
            var data = new SnmpDataFactory().Create(_writeModel, rtuAccident);
            if (data == null) return;
            _snmpService.SendSnmpTrap(_writeModel.SnmpNewSettings, FtTrapType.RtuStatusEventAsSnmp, data);
        }
    }
}
