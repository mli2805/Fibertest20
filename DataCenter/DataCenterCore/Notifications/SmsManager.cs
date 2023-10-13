using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GsmComm.PduConverter;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.DataCenterCore
{
    public class SmsManager
    {
        private readonly IniFile _iniFile;
        private readonly IMyLog _logFile;
        private readonly Model _writeModel;
        private readonly SmsSender _smsSender;
        private readonly int _eventLifetimeLimit;

        public SmsManager(IniFile iniFile, IMyLog logFile, Model writeModel, SmsSender smsSender)
        {
            _iniFile = iniFile;
            _logFile = logFile;
            _writeModel = writeModel;
            _smsSender = smsSender;
            _eventLifetimeLimit = _iniFile.Read(IniSection.Broadcast, IniKey.EventLifetimeLimit, 1800);
        }

        public Task<bool> SendTest(string phoneNumber)
        {
            SendTestSms(phoneNumber);
            return Task.FromResult(true);
        }

        public void SendMonitoringResult(MonitoringResultDto dto)
        {
            var phoneNumbers = _writeModel.GetPhonesToSendMonitoringResult(dto);
            _logFile.AppendLine($"There are {phoneNumbers.Count} numbers to send SMS");
            if (phoneNumbers.Count == 0) return;
            if (DateTime.Now - dto.TimeStamp > TimeSpan.FromMinutes(_eventLifetimeLimit)) return;

            var message = _writeModel.GetShortMessageForMonitoringResult(dto);
            if (message == null) return;

            PushSms(message, phoneNumbers);
        }

        public void SendBopState(BopNetworkEvent cmd)
        {
            var phoneNumbers = _writeModel.GetPhonesToSendBopNetworkEvent(cmd);
            _logFile.AppendLine($"There are {phoneNumbers.Count} numbers to send SMS");
            if (phoneNumbers.Count == 0) return;
            if (DateTime.Now - cmd.EventTimestamp > TimeSpan.FromMinutes(_eventLifetimeLimit)) return;

            var message = EventReport.GetShortMessageForBopState(cmd);
            PushSms(message, phoneNumbers);
        }

        public void SendNetworkEvent(Guid rtuId, bool isMainChannel, bool isOk)
        {
            var phoneNumbers = _writeModel.GetPhonesToSendNetworkEvent(rtuId);
            _logFile.AppendLine($"There are {phoneNumbers.Count} numbers to send SMS");
            if (phoneNumbers.Count == 0) return;

            var message = _writeModel.GetShortMessageForNetworkEvent(rtuId, isMainChannel, isOk);
            PushSms(message, phoneNumbers);
        }

        public void SendRtuStatusEvent(RtuAccident accident)
        {
            var phoneNumbers = _writeModel.GetPhonesToSendRtuStatusEvent(accident.RtuId);
            _logFile.AppendLine($"There are {phoneNumbers.Count} numbers to send SMS");
            if (phoneNumbers.Count == 0) return;

            var message = _writeModel.GetShortMessageForRtuStatusEvent(accident);
            PushSms(message, phoneNumbers);
        }

        private const byte CodeForRussian = 8;

        private void SendTestSms(string phoneNumber)
        {
            var serverIp = _iniFile.Read(IniSection.ServerMainAddress, IniKey.Ip, "");
            PushSms($"{serverIp}: {EventReport.GetTestSms()}", new List<string>() { phoneNumber });
        }

        private void PushSms(string contentOfSms, List<string> phoneNumbers)
        {
            const int limit = 69; 
            foreach (var phoneNumber in phoneNumbers)
            {
                var rest = contentOfSms;
                do
                {
                    var content = rest.Length > limit ? rest.Substring(0, limit) : rest;
                    rest = rest.Length > limit ? rest.Substring(limit) : "";
                    var pdu = new SmsSubmitPdu(content, phoneNumber, CodeForRussian);
                    _smsSender.TheQueue.Enqueue(pdu);

                } while (rest != "");
            }
        }
     
    }
}