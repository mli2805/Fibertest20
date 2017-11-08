using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Iit.Fibertest.DatabaseLibrary;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;

namespace Iit.Fibertest.DataCenterCore
{
    public class LastConnectionTimeChecker
    {
        private readonly IniFile _iniFile;
        private readonly IMyLog _logFile;
        private readonly ClientRegistrationManager _clientRegistrationManager;
        private readonly RtuRegistrationManager _rtuRegistrationManager;
        private readonly D2CWcfManager _d2CWcfManager;

        public LastConnectionTimeChecker(IniFile iniFile, IMyLog logFile,
            ClientRegistrationManager clientRegistrationManager, RtuRegistrationManager rtuRegistrationManager,
            D2CWcfManager d2CWcfManager)
        {
            _iniFile = iniFile;
            _logFile = logFile;
            _clientRegistrationManager = clientRegistrationManager;
            _rtuRegistrationManager = rtuRegistrationManager;
            _d2CWcfManager = d2CWcfManager;
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

                var changes = _rtuRegistrationManager.CheckAndSaveRtuAvailability(rtuHeartbeatPermittedGap).Result;
                if (changes.List.Count != 0)
                {
                    changes.List.ForEach(r => _logFile.AppendLine(r.Report()));
                    var dto = new ListOfRtuWithChangedAvailabilityDto() {List = changes.List};

                    var allClients = _clientRegistrationManager.GetAllLiveClients().Result;
                    if (allClients != null && allClients.Any())
                    {
                        var clientsAddresses = GetClientsAddresses(allClients);
                        _d2CWcfManager.SetClientsAddresses(clientsAddresses);
                        _d2CWcfManager.NotifyAboutRtuChangedAvailability(dto).Wait();
                    }
                }
                Thread.Sleep(checkHeartbeatEvery);
            }

        }

        private List<DoubleAddress> GetClientsAddresses(List<ClientStation> clientStations)
        {
            var result = new List<DoubleAddress>();
            foreach (var clientStation in clientStations)
            {
                result.Add(new DoubleAddress()
                {
                    Main = new NetAddress(clientStation.ClientAddress, clientStation.ClientAddressPort)
                });
            }
            return result;
        }
     
    }
}
