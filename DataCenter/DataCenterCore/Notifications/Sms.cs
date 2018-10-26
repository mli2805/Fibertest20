using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GsmComm.GsmCommunication;
using GsmComm.PduConverter;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.DataCenterCore
{
    public class Sms
    {
        private readonly IniFile _iniFile;
        private readonly IMyLog _logFile;
        private readonly Model _writeModel;

        public Sms(IniFile iniFile, IMyLog logFile, Model writeModel)
        {
            _iniFile = iniFile;
            _logFile = logFile;
            _writeModel = writeModel;
        }

        public async Task<bool> SendTest(string phoneNumber)
        {
            return await Task.Factory.StartNew(() => SendTestSms(phoneNumber)); // here we are waiting result to report user
        }

        public async Task<bool> SendMonitoringResult(MonitoringResultDto dto)
        {
            var phoneNumbers = _writeModel.GetPhonesToSendMonitoringResult(dto);
            _logFile.AppendLine($"There are {phoneNumbers.Count} numbers to send SMS");
            if (phoneNumbers.Count == 0) return true;
            
            var message =  _writeModel.GetShortMessageForMonitoringResult(dto);
            if (message == null) return true;
          
            return await Task.Factory.StartNew(() => SendSms(message, phoneNumbers));
        }

        public async Task<bool> SendBopState(AddBopNetworkEvent cmd)
        {
            var phoneNumbers = _writeModel.GetPhonesToSendBopNetworkEvent(cmd);
            _logFile.AppendLine($"There are {phoneNumbers.Count} numbers to send SMS");
            if (phoneNumbers.Count == 0) return true;

            var message = _writeModel.GetShortMessageForBopState(cmd);
            return await Task.Factory.StartNew(() => SendSms(message, phoneNumbers));
        }

        public async Task<bool> SendNetworkEvent(Guid rtuId, bool isMainChannel, bool isOk)
        {
            var phoneNumbers = _writeModel.GetPhonesToSendNetworkEvent(rtuId);
         
            _logFile.AppendLine($"There are {phoneNumbers.Count} numbers to send SMS");
            if (phoneNumbers.Count == 0) return true;

            var message = _writeModel.GetShortMessageForNetworkEvent(rtuId, isMainChannel, isOk);
            return await Task.Factory.StartNew(() => SendSms(message, phoneNumbers));
        }


        private const string ContentOfTestSms = "Test SMS message - Тестовое СМС сообщение";
        private const byte CodeForRussian = 8;

        private bool SendTestSms(string phoneNumber)
        {
            var serverIp = _iniFile.Read(IniSection.ServerMainAddress, IniKey.Ip, "");
            return SendSms($"{serverIp}: {ContentOfTestSms}", new List<string>() { phoneNumber });
        }

        private bool SendSms(string contentOfSms, List<string> phoneNumbers)
        {
            var comPort = _iniFile.Read(IniSection.Broadcast, IniKey.GsmModemComPort, 0);
            var comm = new GsmCommMain($"COM{comPort}", 9600, 150);
            try
            {
                comm.Open();
            }
            catch (Exception e)
            {
                _logFile.AppendLine("comm.Open: " + e.Message);
                return false;
            }

            var flag = true;
            try
            {
                foreach (var phoneNumber in phoneNumbers)
                {
                    var rest = contentOfSms;
                    do
                    {
                        var content = rest.Length > 63 ? rest.Substring(0, 63) : rest;
                        rest = rest.Length > 63 ? rest.Substring(63) : "";
                        var pdu = new SmsSubmitPdu(content, phoneNumber, CodeForRussian);
                        comm.SendMessage(pdu);

                    } while (rest != "");
                }
            }
            catch (Exception e)
            {
                _logFile.AppendLine("comm.SendMessage: " + e.Message);
                flag = false;
            }

            try
            {
                comm.Close();
            }
            catch (Exception e)
            {
                _logFile.AppendLine("comm.Close: " + e.Message);
                flag = false;
            }

            return flag;
        }
    }
}