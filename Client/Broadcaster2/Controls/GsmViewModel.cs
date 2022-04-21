using System;
using System.Windows;
using Caliburn.Micro;
using GsmComm.GsmCommunication;
using GsmComm.PduConverter;
using Iit.Fibertest.UtilsLib;

namespace Broadcaster2
{
    public class GsmViewModel : PropertyChangedBase
    {
        private readonly IniFile _iniFile;
        public int GsmComPort { get; set; }
        public string SendToNumber { get; set; }
        public string ContentOfSms { get; set; }

        public GsmViewModel(IniFile iniFile)
        {
            _iniFile = iniFile;

            GsmComPort = _iniFile.Read(IniSection.Broadcast, IniKey.GsmModemComPort, 3);
            SendToNumber = _iniFile.Read(IniSection.Broadcast, IniKey.TestNumberToSms, "+375291234567");
            ContentOfSms = _iniFile.Read(IniSection.Broadcast, IniKey.TestSmsContent, "Fibertest 2.0 Test SMS Тестовая СМСка");
        }

        public void CheckModemConnection()
        {
            var comm = new GsmCommMain($"COM{GsmComPort}", 9600, 150);
            try
            {
                comm.Open();
                MessageBox.Show("Modem connected successfully");
                _iniFile.Write(IniSection.Broadcast, IniKey.GsmModemComPort, GsmComPort);
                comm.Close();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        private const byte CodeForRussian = 8;
        // public const byte Code7Bit = (byte) DataCodingScheme.GeneralCoding.Alpha7BitDefault;
        public void SendSms()
        {
            var comm = new GsmCommMain($"COM{GsmComPort}", 9600, 150);
            try
            {
                _iniFile.Write(IniSection.Broadcast, IniKey.TestNumberToSms, SendToNumber);
                _iniFile.Write(IniSection.Broadcast, IniKey.TestSmsContent, ContentOfSms);
                comm.Open();
                var pdu = new SmsSubmitPdu(ContentOfSms, SendToNumber, CodeForRussian);
                comm.SendMessage(pdu);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
            comm.Close();
        }
    }
}
