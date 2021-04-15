using System;
using System.Threading;
using System.Threading.Tasks;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.DataCenterCore
{
    // https://stackoverflow.com/questions/66496547/signalr-and-or-timer-issues-since-chrome-88
    // temporary workaround chromium timer throttling problem
    public class SignalRNudger
    {
        private readonly IniFile _iniFile;
        private readonly IFtSignalRClient _ftSignalRClient;
        private TimeSpan _nudgeSignalrTimeout;

        public SignalRNudger(IniFile iniFile, IFtSignalRClient ftSignalRClient)
        {
            _iniFile = iniFile;
            _ftSignalRClient = ftSignalRClient;
        }

        public void Start()
        {
            var thread = new Thread(Check) { IsBackground = true };
            thread.Start();
        }

        // ReSharper disable once FunctionNeverReturns 
        private void Check()
        {
            _nudgeSignalrTimeout = TimeSpan.FromSeconds(_iniFile.Read(IniSection.General, IniKey.NudgeSignalrTimeout, 20));
            while (true)
            {
                Tick().Wait();
                Thread.Sleep(_nudgeSignalrTimeout);
            }
        }

        private async Task Tick()
        {
            await _ftSignalRClient.NotifyAll("NudgeSignalR", null);
        }


    }
}
