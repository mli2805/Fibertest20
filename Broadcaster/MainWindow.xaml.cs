using System;
using System.Windows;
using GsmComm.GsmCommunication;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WpfCommonViews;

namespace Broadcaster
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private IniFile _iniFile;

        public int GsmComPort { get; set; }
      
        public string SendToNumber { get; set; }
        public string ContentOfSms { get; set; }

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
            SendToNumber = _iniFile.Read(IniSection.Broadcast, IniKey.TestNumberToSms, "+375292788154");
            ContentOfSms = _iniFile.Read(IniSection.Broadcast, IniKey.TestSmsContent, "Fibertest 2.0 Test SMS");
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

        private void SendSms(object sender, RoutedEventArgs e)
        {

        }
    }
}
