using System;
using System.Threading;
using Iit.Fibertest.DatabaseLibrary;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.DataCenterCore
{
    public class LastConnectionTimeChecker
    {
        private readonly IniFile _iniFile;
        private readonly ClientRegistrationManager _clientRegistrationManager;

        public LastConnectionTimeChecker(IniFile iniFile, ClientRegistrationManager clientRegistrationManager)
        {
            _iniFile = iniFile;
            _clientRegistrationManager = clientRegistrationManager;
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
