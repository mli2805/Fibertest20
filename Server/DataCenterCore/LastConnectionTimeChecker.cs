using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Iit.Fibertest.DatabaseLibrary;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.DataCenterCore
{
    public class LastConnectionTimeChecker
    {
        private readonly IniFile _iniFile;
        private readonly IMyLog _logFile;
        private readonly ClientRegistrationManager _clientRegistrationManager;

        public LastConnectionTimeChecker(IniFile iniFile, IMyLog logFile, ClientRegistrationManager clientRegistrationManager)
        {
            _logFile = logFile;
            _clientRegistrationManager = clientRegistrationManager;
            _iniFile = iniFile;
        }

        public void Start()
        {
            var thread = new Thread(Check) { IsBackground = true };
            thread.Start();

        }

        private void Check()
        {
            var checkHeartbeatEvery =
                TimeSpan.FromSeconds(_iniFile.Read(IniSection.General, IniKey.CheckHeartbeatEvery, 3));
            var rtuHeartbeatPermittedGap =
                TimeSpan.FromSeconds(_iniFile.Read(IniSection.General, IniKey.RtuHeartbeatPermittedGap, 70));
            var clientHeartbeatPermittedGap =
                TimeSpan.FromSeconds(_iniFile.Read(IniSection.General, IniKey.ClientHeartbeatPermittedGap, 180));

            while (true)
            {

                _clientRegistrationManager.CleanDeadClients(clientHeartbeatPermittedGap).Wait();

                Thread.Sleep(checkHeartbeatEvery);
            }

        }

       
    }
}
