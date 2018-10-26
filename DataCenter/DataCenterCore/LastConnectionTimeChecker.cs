using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GsmComm.PduConverter;
using Iit.Fibertest.DatabaseLibrary;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.DataCenterCore
{
    public class SmsQueue
    {
        public ConcurrentQueue<SmsSubmitPdu> TheQueue;
    }
    public class SmsSender
    {
        
    }

    public class LastConnectionTimeChecker
    {
        private readonly IniFile _iniFile;
        private readonly IMyLog _logFile;
        private readonly EventStoreService _eventStoreService;
        private readonly ClientsCollection _clientsCollection;
        private readonly RtuStationsRepository _rtuStationsRepository;
        private readonly Model _writeModel;
        private readonly Smtp _smtp;
        private readonly Sms _sms;
        private TimeSpan _checkHeartbeatEvery;
        private TimeSpan _rtuHeartbeatPermittedGap;
        private TimeSpan _clientHeartbeatPermittedGap;

        public LastConnectionTimeChecker(IniFile iniFile, IMyLog logFile, EventStoreService eventStoreService,
            ClientsCollection clientsCollection, RtuStationsRepository rtuStationsRepository, Model writeModel,
            Smtp smtp, Sms sms)
        {
            _iniFile = iniFile;
            _logFile = logFile;
            _eventStoreService = eventStoreService;
            _clientsCollection = clientsCollection;
            _rtuStationsRepository = rtuStationsRepository;
            _writeModel = writeModel;
            _smtp = smtp;
            _sms = sms;
        }

        public void Start()
        {
            var thread = new Thread(Check) { IsBackground = true };
            thread.Start();
        }

        // ReSharper disable once FunctionNeverReturns 
        private void Check()
        {
            _checkHeartbeatEvery = TimeSpan.FromSeconds(_iniFile.Read(IniSection.General, IniKey.CheckHeartbeatEvery, 3));
            _rtuHeartbeatPermittedGap = TimeSpan.FromSeconds(_iniFile.Read(IniSection.General, IniKey.RtuHeartbeatPermittedGap, 70));
            _clientHeartbeatPermittedGap = TimeSpan.FromSeconds(_iniFile.Read(IniSection.General, IniKey.ClientConnectionsPermittedGap, 180));

            // if server just started it should give RTUs time to check-in
            Thread.Sleep(_rtuHeartbeatPermittedGap);

            while (true)
            {
                Tick().Wait();
                Thread.Sleep(_checkHeartbeatEvery);
            }
        }

        private async Task<int> Tick()
        {
            _clientsCollection.CleanDeadClients(_clientHeartbeatPermittedGap);

            var networkEvents = await GetNewNetworkEvents(_rtuHeartbeatPermittedGap);
            if (networkEvents.Count == 0)
                return 0;

            foreach (var networkEvent in networkEvents)
            {
                var command = new AddNetworkEvent()
                {
                    RtuId = networkEvent.RtuId,
                    EventTimestamp = DateTime.Now,
                    MainChannelState = networkEvent.MainChannelState,
                    ReserveChannelState = networkEvent.ReserveChannelState,
                };
                await _eventStoreService.SendCommand(command, "system", "OnServer");
            }

            return 0;
        }


        private async Task<List<NetworkEvent>> GetNewNetworkEvents(TimeSpan rtuHeartbeatPermittedGap)
        {
            DateTime noLaterThan = DateTime.Now - rtuHeartbeatPermittedGap;
            var stations = await _rtuStationsRepository.GetAllRtuStations();

            List<RtuStation> changedStations = new List<RtuStation>();
            List<NetworkEvent> networkEvents = new List<NetworkEvent>();
            foreach (var rtuStation in stations)
            {
                NetworkEvent networkEvent = await CheckRtuStation(rtuStation, noLaterThan);
                if (networkEvent != null)
                {
                    changedStations.Add(rtuStation);
                    networkEvents.Add(networkEvent);
                }
            }
            if (changedStations.Count > 0)
                await _rtuStationsRepository.SaveAvailabilityChanges(changedStations);

            return networkEvents;
        }

        private async Task<NetworkEvent> CheckRtuStation(RtuStation rtuStation, DateTime noLaterThan)
        {
            var networkEvent = new NetworkEvent() { RtuId = rtuStation.RtuGuid, EventTimestamp = DateTime.Now };

            var flag = await CheckMainChannel(rtuStation, noLaterThan, networkEvent);

            if (rtuStation.IsReserveAddressSet)
                flag = await CheckReserveChannel(rtuStation, noLaterThan, networkEvent);

            return flag ? networkEvent : null;
        }

        private async Task<bool> CheckReserveChannel(RtuStation rtuStation, DateTime noLaterThan, NetworkEvent networkEvent)
        {
            var rtuTitle = _writeModel.Rtus.First(r => r.Id == rtuStation.RtuGuid).Title;
            if (rtuStation.LastConnectionByReserveAddressTimestamp < noLaterThan &&
                rtuStation.IsReserveAddressOkDuePreviousCheck)
            {
                rtuStation.IsMainAddressOkDuePreviousCheck = false;
                networkEvent.ReserveChannelState = RtuPartState.Broken;
                _logFile.AppendLine($"RTU \"{rtuTitle}\" Reserve channel - Broken");
                await _smtp.SendNetworkEvent(rtuStation.RtuGuid, false, false);
                await _sms.SendNetworkEvent(rtuStation.RtuGuid, false, false);
                return true;
            }

            if (rtuStation.LastConnectionByReserveAddressTimestamp >= noLaterThan &&
                !rtuStation.IsReserveAddressOkDuePreviousCheck)
            {
                rtuStation.IsMainAddressOkDuePreviousCheck = true;
                networkEvent.ReserveChannelState = RtuPartState.Ok;
                _logFile.AppendLine($"RTU \"{rtuTitle}\" Reserve channel - Recovered");
                await _smtp.SendNetworkEvent(rtuStation.RtuGuid, false, true);
                await _sms.SendNetworkEvent(rtuStation.RtuGuid, false, true);
                return true;
            }
            return false;
        }

        private async Task<bool> CheckMainChannel(RtuStation rtuStation, DateTime noLaterThan, NetworkEvent networkEvent)
        {
            var rtuTitle = _writeModel.Rtus.First(r => r.Id == rtuStation.RtuGuid).Title;
            if (rtuStation.LastConnectionByMainAddressTimestamp < noLaterThan && rtuStation.IsMainAddressOkDuePreviousCheck)
            {
                rtuStation.IsMainAddressOkDuePreviousCheck = false;
                networkEvent.MainChannelState = RtuPartState.Broken;
                _logFile.AppendLine($"RTU \"{rtuTitle}\" Main channel - Broken");
                await _smtp.SendNetworkEvent(rtuStation.RtuGuid, true, false);
                await _sms.SendNetworkEvent(rtuStation.RtuGuid, true, false);
                return true;
            }

            if (rtuStation.LastConnectionByMainAddressTimestamp >= noLaterThan && !rtuStation.IsMainAddressOkDuePreviousCheck)
            {
                rtuStation.IsMainAddressOkDuePreviousCheck = true;
                networkEvent.MainChannelState = RtuPartState.Ok;
                _logFile.AppendLine($"RTU \"{rtuTitle}\" Main channel - Recovered");
                await _smtp.SendNetworkEvent(rtuStation.RtuGuid, true, true);
                await _sms.SendNetworkEvent(rtuStation.RtuGuid, true, true);
                return true;
            }
            return false;
        }
    }
}
