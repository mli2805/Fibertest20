using System;
using System.Threading;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;

namespace Iit.Fibertest.Client
{
    public class Heartbeater
    {
        private readonly IMyLog _logFile;
        private readonly IWcfServiceDesktopC2D _wcfConnection;
        private readonly CurrentUser _currentUser;
        private readonly int _heartbeatRate;
        private Thread _heartbeaterThread;
        public CancellationTokenSource CancellationTokenSource { get; set; }

        public Heartbeater(IniFile iniFile, IMyLog logFile, 
            IWcfServiceDesktopC2D wcfConnection, CurrentUser currentUser)
        {
            _logFile = logFile;
            _wcfConnection = wcfConnection;
            _currentUser = currentUser;
            _heartbeatRate = iniFile.Read(IniSection.General, IniKey.ClientHeartbeatRateMs, 1000);
        }

        public void Start()
        {
            _logFile.AppendLine(@"Heartbeats started");
            _heartbeaterThread = new Thread(SendHeartbeats) { IsBackground = true };
            _heartbeaterThread.Start();
        }

        private async void SendHeartbeats()
        {
            while (!CancellationTokenSource.IsCancellationRequested)
            {
                await _wcfConnection.SendHeartbeat(new HeartbeatDto(){ConnectionId = _currentUser.ConnectionId});
                Thread.Sleep(TimeSpan.FromMilliseconds(_heartbeatRate));
            }
            _logFile.AppendLine(@"Leaving Heartbeats...");
        }
    }
}
