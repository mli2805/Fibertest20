﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GsmComm.GsmCommunication;
using GsmComm.PduConverter;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.DataCenterCore
{
    public class HtmlReport
    {

    }
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
            var phoneNumbers = _writeModel.Users
                .Where(u => u.Sms.IsActivated && u.Sms.ShouldUserReceiveMoniResult(dto.TraceState))
                .Select(u => u.Sms.PhoneNumber).ToList();

            _logFile.AppendLine($"There are {phoneNumbers.Count} numbers to send SMS");
            if (phoneNumbers.Count == 0) return true;
            
            var message =  _writeModel.GetShortMessageForMonitoringResult(dto);
            if (message == null) return true;
          
            // ReSharper disable once UnusedVariable
            var task = Task.Factory.StartNew(() => SendSms(message, phoneNumbers)); // here we do not wait result
            await Task.Delay(1);
            return true;
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
                comm.Close();
                return true;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("SendTestSms: " + e.Message);
                comm.Close();
                return false;
            }
        }
    }
}