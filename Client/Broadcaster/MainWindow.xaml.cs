﻿using System;
using System.ComponentModel;
using System.IO;
using System.Messaging;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using Broadcaster.Properties;
using GsmComm.GsmCommunication;
using GsmComm.PduConverter;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WpfCommonViews;

namespace Broadcaster
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INotifyPropertyChanged
    {
        private IniFile _iniFile;
        private int _sentCount;

        public int GsmComPort { get; set; }

        public string SendToNumber { get; set; }
        public string ContentOfSms { get; set; }

        public string ServerIp { get; set; } = "192.168.96.21";
        public string SorFileName { get; set; } = @"..\file.sor";
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
        }

        private void CheckModemConnection(object sender, RoutedEventArgs e)
        {
            var comm = new GsmCommMain($"COM{GsmComPort}", 9600, 150);
            try
            {
                using (new WaitCursor())
                {
                    comm.Open();
                    MessageBox.Show("Modem connected successfully");
                    _iniFile.Write(IniSection.Broadcast, IniKey.GsmModemComPort, GsmComPort);
                    comm.Close();
                }
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
                using (new WaitCursor())
                {
                    _iniFile.Write(IniSection.Broadcast, IniKey.TestNumberToSms, SendToNumber);
                    _iniFile.Write(IniSection.Broadcast, IniKey.TestSmsContent, ContentOfSms);
                    comm.Open();
                    var pdu = new SmsSubmitPdu(ContentOfSms, SendToNumber, CodeForRussian);
                    comm.SendMessage(pdu);
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
            comm.Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Task.Factory.StartNew(VeryLongOperation);
        }

        private async void VeryLongOperation()
        {
            SentCount = 0;
            var dto = CreateDto();

            var connectionString = $@"FormatName:DIRECT=TCP:{ServerIp}\private$\Fibertest20";
            var queue = new MessageQueue(connectionString);

            Message message = new Message(dto, new BinaryMessageFormatter());

            for (int i = 0; i < MsmqCount; i++)
            {
                queue.Send(message, MessageQueueTransactionType.Single);
                Application.Current.Dispatcher.Invoke(() => SentCount++);
                await Task.Delay(MsmqPauseMs);
            }
        }

        private MonitoringResultDto CreateDto()
        {
            var bytes = File.ReadAllBytes(SorFileName);
            var dto = new MonitoringResultDto()
            {
                RtuId = Guid.Parse("00bf8f28-345f-44dc-af18-1ba6d9e4c563"),
                TimeStamp = DateTime.Now,
                PortWithTrace = new PortWithTraceDto()
                {
                    TraceId = Guid.Parse("d7f1f9ab-23bc-418d-82e6-ff755fc2b469"),
                    OtauPort = new OtauPortDto() {OpticalPort = 2, Serial = "68613", IsPortOnMainCharon = true,},
                },
                BaseRefType = BaseRefType.Precise,
                TraceState = FiberState.Ok,
                SorBytes = bytes,
            };
            return dto;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}