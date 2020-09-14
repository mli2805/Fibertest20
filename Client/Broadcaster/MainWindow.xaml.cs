using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Messaging;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Broadcaster.Properties;
using GsmComm.GsmCommunication;
using GsmComm.PduConverter;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;

namespace Broadcaster
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INotifyPropertyChanged
    {
        private IniFile _iniFile;
        private IMyLog _logFile;
        private int _sentCount;

        public int GsmComPort { get; set; }

        public string SendToNumber { get; set; }
        public string ContentOfSms { get; set; }

        public string ServerIp { get; set; } = "192.168.96.21";
        public string SorFileName { get; set; } = @"..\file.sor";
        public string SorBreakFileName { get; set; } = @"..\fileBreak.sor";
        public Guid RtuId;
        public Guid TraceId;
        public int MsmqCount { get; set; } = 1;
        public int MsmqPauseMs { get; set; } = 2000;

        public int SentCount
        {
            get { return _sentCount; }
            set
            {
                if (value == _sentCount) return;
                _sentCount = value;
                OnPropertyChanged();
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _iniFile = new IniFile();
            _iniFile.AssignFile("broadcaster.ini");
            GsmComPort = _iniFile.Read(IniSection.Broadcast, IniKey.GsmModemComPort, 3);
            SendToNumber = _iniFile.Read(IniSection.Broadcast, IniKey.TestNumberToSms, "+375291234567");
            ContentOfSms = _iniFile.Read(IniSection.Broadcast, IniKey.TestSmsContent, "Fibertest 2.0 Test SMS Тестовая СМСка");
            _logFile = new LogFile(_iniFile);
            _logFile.AssignFile("broadcaster.log");

            SelectedSnmpEncoding = SnmpEncodings[2];
        }

        private void CheckModemConnection(object sender, RoutedEventArgs e)
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
        private void SendSms(object sender, RoutedEventArgs e)
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var s = _iniFile.Read(IniSection.Broadcast, IniKey.MsmqTestRtuId, "00bf8f28-345f-44dc-af18-1ba6d9e4c563");
            RtuId = Guid.Parse(s);
            s = _iniFile.Read(IniSection.Broadcast, IniKey.MsmqTestTraceId, "d7f1f9ab-23bc-418d-82e6-ff755fc2b469");
            TraceId = Guid.Parse(s);
            Task.Factory.StartNew(VeryLongOperation);
        }

        private void VeryLongOperation()
        {
            SentCount = 0;
            var dto = CreateDto(false);
            var dtoBroken = CreateDto(true);

            var connectionString = $@"FormatName:DIRECT=TCP:{ServerIp}\private$\Fibertest20";
            var queue = new MessageQueue(connectionString);

            Message message = new Message(dto, new BinaryMessageFormatter());
            Message messageBroken = new Message(dtoBroken, new BinaryMessageFormatter());
            IncreaseSentCounter del = F;
            for (int i = 0; i < MsmqCount; i++)
            {
                queue.Send(i % 4 == 0 ? messageBroken : message, MessageQueueTransactionType.Single);
                Application.Current.Dispatcher?.Invoke(del);
                Thread.Sleep(MsmqPauseMs);
            }
        }

        private delegate void IncreaseSentCounter();
        private void F()
        {
            SentCount++;
        }

        private MonitoringResultDto CreateDto(bool isBroken)
        {
            var bytes = File.ReadAllBytes(isBroken ? SorBreakFileName : SorFileName);
            var dto = new MonitoringResultDto()
            {
                RtuId = RtuId,
                TimeStamp = DateTime.Now,
                PortWithTrace = new PortWithTraceDto()
                {
                    TraceId = TraceId,
                    OtauPort = new OtauPortDto() { OpticalPort = 2, Serial = "68613", IsPortOnMainCharon = true, },
                },
                BaseRefType = BaseRefType.Precise,
                TraceState = isBroken ? FiberState.FiberBreak : FiberState.Ok,
                SorBytes = bytes,
            };
            return dto;
        }



        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private int snmpTrapVersion = 1;
        public string SnmpManagerIp { get; set; } = "192.168.96.21";
        public int SnmpManagerPort { get; set; } = 162;
        public string SnmpCommunity { get; set; } = "IIT";

        public List<string> SnmpEncodings { get; set; } = new List<string>(){ "unicode (utf16)", "utf8", "windows1251"};

        public string SelectedSnmpEncoding { get; set; }
        public string EnterpriseOid { get; set; } = "1.3.6.1.4.1.36220";

        private void SendV1TestTrap(object sender, RoutedEventArgs e)
        {
            // save all user's input into ini-file: snmpAgent will read them from ini-file
            SaveInputs();

            var snmpAgent = new SnmpAgent(_iniFile, _logFile);
            snmpAgent.SendTestTrap();
        }

        private void SaveInputs()
        {
            _iniFile.Write(IniSection.Snmp, IniKey.SnmpTrapVersion, snmpTrapVersion);

            _iniFile.Write(IniSection.Snmp, IniKey.SnmpReceiverIp, SnmpManagerIp);
            _iniFile.Write(IniSection.Snmp, IniKey.SnmpReceiverPort, SnmpManagerPort);
            _iniFile.Write(IniSection.Snmp, IniKey.SnmpCommunity, SnmpCommunity);
            _iniFile.Write(IniSection.Snmp, IniKey.SnmpEncoding, SelectedSnmpEncoding);

            _iniFile.Write(IniSection.Snmp, IniKey.EnterpriseOid, EnterpriseOid);

        }
    }
}