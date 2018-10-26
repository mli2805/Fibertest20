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

        public SmsManager(IniFile iniFile, IMyLog logFile, Model writeModel, SmsSender smsSender)
        {
            _iniFile = iniFile;
            _logFile = logFile;
            _writeModel = writeModel;
            _smsSender = smsSender;
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

            var message = _writeModel.GetShortMessageForMonitoringResult(dto);
            if (message == null) return;

            PushSms(message, phoneNumbers);
        }

        public void SendBopState(AddBopNetworkEvent cmd)
        {
            var phoneNumbers = _writeModel.GetPhonesToSendBopNetworkEvent(cmd);
            _logFile.AppendLine($"There are {phoneNumbers.Count} numbers to send SMS");
            if (phoneNumbers.Count == 0) return;

            var message = _writeModel.GetShortMessageForBopState(cmd);
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


        private const string ContentOfTestSms = "Test SMS message - Тестовое СМС сообщение";
        private const byte CodeForRussian = 8;

        private void SendTestSms(string phoneNumber)
        {
            var serverIp = _iniFile.Read(IniSection.ServerMainAddress, IniKey.Ip, "");
            PushSms($"{serverIp}: {ContentOfTestSms}", new List<string>() { phoneNumber });
        }

        private void PushSms(string contentOfSms, List<string> phoneNumbers)
        {
            foreach (var phoneNumber in phoneNumbers)
            {
                var rest = contentOfSms;
                do
                {
                    var content = rest.Length > 63 ? rest.Substring(0, 63) : rest;
                    rest = rest.Length > 63 ? rest.Substring(63) : "";
                    var pdu = new SmsSubmitPdu(content, phoneNumber, CodeForRussian);
                    _smsSender.TheQueue.Enqueue(pdu);

                } while (rest != "");
            }
        }
    }
}