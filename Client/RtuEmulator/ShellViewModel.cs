using System;
using System.Collections.Generic;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using System.Messaging;

namespace RtuEmulator {
    public class ShellViewModel : Caliburn.Micro.Screen, IShell
    {
        private IniFile _iniFile;
        private IMyLog _logFile;

        public string ServerIp { get; set; }
        public string RtuId { get; set; }
        public string TraceId { get; set; }
        public int MainCharonPort { get; set; }

        public List<ReturnCode> ReturnCodes { get; set; } = new List<ReturnCode>()
        {
            ReturnCode.MeasurementEndedNormally,
            ReturnCode.MeasurementFailedToSetParametersFromBase,
            ReturnCode.MeasurementBaseRefNotFound,
        };

        public ReturnCode SelectedReturnCode { get; set; } = ReturnCode.MeasurementEndedNormally;

        public ShellViewModel()
        {
            _iniFile = new IniFile();
            _iniFile.AssignFile("rtuEmulator.ini");
            _logFile = new LogFile(_iniFile);
            _logFile.AssignFile("rtuEmulator.log");

            Initialize();
        }

        private void Initialize()
        {
            ServerIp = _iniFile.Read(IniSection.ServerMainAddress, IniKey.Ip, "192.168.115.189");

            RtuId = _iniFile.Read(IniSection.RtuEmulator, IniKey.RtuId, "");
            TraceId = _iniFile.Read(IniSection.RtuEmulator, IniKey.TraceId, "");
            MainCharonPort = _iniFile.Read(IniSection.RtuEmulator, IniKey.MainCharonPort, 1);
        }

        public void SaveSettings()
        {
            _iniFile.Write(IniSection.ServerMainAddress, IniKey.Ip, ServerIp);
            _iniFile.Write(IniSection.RtuEmulator, IniKey.RtuId, RtuId);
            _iniFile.Write(IniSection.RtuEmulator, IniKey.TraceId, TraceId);
            _iniFile.Write(IniSection.RtuEmulator, IniKey.MainCharonPort, MainCharonPort);
        }

        public void SendDtoByMsmq()
        {
            var dto = CreateDto();
            var connectionString = $@"FormatName:DIRECT=TCP:{ServerIp}\private$\Fibertest20";
            var queue = new MessageQueue(connectionString);
            Message message = new Message(dto, new BinaryMessageFormatter());
            queue.Send(message, MessageQueueTransactionType.Single);
            _logFile.AppendLine("Monitoring result sent by MSMQ."); }

        private MonitoringResultDto CreateDto()
        {
            return new MonitoringResultDto()
            {
                ReturnCode = SelectedReturnCode,
                Reason = ReasonToSendMonitoringResult.MeasurementAccidentStatusChanged,

                RtuId = Guid.Parse(RtuId),
                BaseRefType = BaseRefType.Fast,

                PortWithTrace = new PortWithTraceDto()
                {
                    TraceId = Guid.Parse(TraceId),
                    OtauPort = new OtauPortDto()
                    {
                        IsPortOnMainCharon = true,
                        MainCharonPort = MainCharonPort,
                    }
                },
                TimeStamp = DateTime.Now,
            };
        }
    }
}