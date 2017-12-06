using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Iit.Fibertest.DatabaseLibrary;
using Iit.Fibertest.DatabaseLibrary.DbContexts;
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
        private TimeSpan _checkHeartbeatEvery;
        private TimeSpan _rtuHeartbeatPermittedGap;
        private TimeSpan _clientHeartbeatPermittedGap;

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
            _checkHeartbeatEvery = TimeSpan.FromSeconds(_iniFile.Read(IniSection.General, IniKey.CheckHeartbeatEvery, 3));
            _rtuHeartbeatPermittedGap = TimeSpan.FromSeconds(_iniFile.Read(IniSection.General, IniKey.RtuHeartbeatPermittedGap, 70));
            _clientHeartbeatPermittedGap = TimeSpan.FromSeconds(_iniFile.Read(IniSection.General, IniKey.ClientHeartbeatPermittedGap, 180));

            while (true)
            {
                Tick().Wait();
                Thread.Sleep(_checkHeartbeatEvery);
            }
        }

        private async Task<int> Tick()
        {
            _clientRegistrationManager.CleanDeadClients(_clientHeartbeatPermittedGap).Wait();

            var changes = await _rtuRegistrationManager.GetAndSaveRtuStationsAvailabilityChanges(_rtuHeartbeatPermittedGap);
            if (changes.List.Count == 0)
                return 0;

            await RecordNetworkEvents(changes);
            await NotifyClientsRtuAvailabilityChanged(changes);
            return 0;
        }

        private async Task<int> RecordNetworkEvents(RtuWithChannelChangesList changes)
        {
            try
            {
                var dbContext = new MySqlContext();
                foreach (var rtuWithChannelChanges in changes.List)
                {
                    dbContext.NetworkEvents.Add(new NetworkEvent()
                    {
                        RtuId = rtuWithChannelChanges.RtuId,
                        EventTimestamp = DateTime.Now,
                        MainChannelState = rtuWithChannelChanges.MainChannel,
                        ReserveChannelState = rtuWithChannelChanges.ReserveChannel,
                        BopString = "", // rabbish
                    });
                }
                return await dbContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logFile.AppendLine("RecordNetworkEvents:" + e.Message);
                return -1;
            }

        }
        private async Task<int> NotifyClientsRtuAvailabilityChanged(RtuWithChannelChangesList changes)
        {
            changes.List.ForEach(r => _logFile.AppendLine(r.Report()));
            var dto = new ListOfRtuWithChangedAvailabilityDto() { List = changes.List };

            var addresses = await _clientRegistrationManager.GetClientsAddresses();
            if (addresses == null)
                return 0;
            _d2CWcfManager.SetClientsAddresses(addresses);
            return await _d2CWcfManager.NotifyAboutRtuChangedAvailability(dto);
        }
    }
}
