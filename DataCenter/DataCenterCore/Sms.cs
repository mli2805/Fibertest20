using System;
using System.Linq;
using System.Threading.Tasks;
using GsmComm.GsmCommunication;
using GsmComm.PduConverter;
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

        public async Task<bool> SendTestSms(int comPort)
        {
            _iniFile.Write(IniSection.Broadcast, IniKey.GsmModemComPort, comPort);
            return await Task.Factory.StartNew(SendTestSms);
        }

        private const string ContentOfSms = "Test SMS message - Тестовое СМС сообщение";
        private const byte CodeForRussian = 8;

        private bool SendTestSms()
        {
            var comPort = _iniFile.Read(IniSection.Broadcast, IniKey.GsmModemComPort, 0);
            var comm = new GsmCommMain($"COM{comPort}", 9600, 150);
            try
            {
                comm.Open();
                foreach (var phoneNumber in _writeModel.Users.Where(u => u.Sms.IsActivated)
                    .Select(u => u.Sms.PhoneNumber))
                {
                    var pdu = new SmsSubmitPdu(ContentOfSms, phoneNumber, CodeForRussian);
                    comm.SendMessage(pdu);
                }
            }
            catch (Exception e)
            {
                _logFile.AppendLine("SendTestSms: " + e.Message);
                throw;
            }

            comm.Close();
            return true;
        }
    }
}