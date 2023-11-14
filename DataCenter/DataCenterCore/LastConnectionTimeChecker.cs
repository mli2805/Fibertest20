using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Iit.Fibertest.DatabaseLibrary;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.DataCenterCore
{
    public class LastConnectionTimeChecker
    {
        private static readonly IMapper Mapper = new MapperConfiguration(
            cfg => cfg.AddProfile<MappingWebApiProfile>()).CreateMapper();

        private readonly IniFile _iniFile;
        private readonly IMyLog _logFile;
        private readonly GlobalState _globalState;
        private readonly EventStoreService _eventStoreService;
        private readonly ClientsCollection _clientsCollection;
        private readonly RtuStationsRepository _rtuStationsRepository;
        private readonly Model _writeModel;
        private readonly IFtSignalRClient _ftSignalRClient;
        private readonly Smtp _smtp;
        private readonly SmsManager _smsManager;
        private readonly SnmpNotifier _snmpNotifier;
        private TimeSpan _checkHeartbeatEvery;
        private TimeSpan _rtuHeartbeatPermittedGap;
        private TimeSpan _clientHeartbeatPermittedGap;

        public LastConnectionTimeChecker(IniFile iniFile, IMyLog logFile, GlobalState globalState,
            EventStoreService eventStoreService, ClientsCollection clientsCollection,
            RtuStationsRepository rtuStationsRepository, Model writeModel,
            IFtSignalRClient ftSignalRClient,
            Smtp smtp, SmsManager smsManager, SnmpNotifier snmpNotifier)
        {
            _iniFile = iniFile;
            _logFile = logFile;
            _globalState = globalState;
            _eventStoreService = eventStoreService;
            _clientsCollection = clientsCollection;
            _rtuStationsRepository = rtuStationsRepository;
            _writeModel = writeModel;
            _ftSignalRClient = ftSignalRClient;
            _smtp = smtp;
            _smsManager = smsManager;
            _snmpNotifier = snmpNotifier;
        }

        public void Start()
        {
            var thread = new Thread(Check) { IsBackground = true };
            thread.Start();
        }

        // ReSharper disable once FunctionNeverReturns 
        private void Check()
        {
            var currentCulture = _iniFile.Read(IniSection.General, IniKey.Culture, @"ru-RU");
            Thread.CurrentThread.CurrentCulture = new CultureInfo(currentCulture);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(currentCulture);

            _checkHeartbeatEvery = TimeSpan.FromSeconds(_iniFile.Read(IniSection.General, IniKey.CheckHeartbeatEvery, 3));
            _rtuHeartbeatPermittedGap = TimeSpan.FromSeconds(_iniFile.Read(IniSection.General, IniKey.RtuHeartbeatPermittedGap, 70));
            _clientHeartbeatPermittedGap = TimeSpan.FromSeconds(_iniFile.Read(IniSection.General, IniKey.ClientConnectionsPermittedGap, 180));

            // if server just started it should give RTUs time to check-in
            Thread.Sleep(_rtuHeartbeatPermittedGap);
            // Thread.Sleep(10000);

            while (true)
            {
                if (!_globalState.IsDatacenterInDbOptimizationMode)
                    Tick().Wait();
                Thread.Sleep(_checkHeartbeatEvery);
            }
        }

        private async Task<int> Tick()
        {
            // _logFile.AppendLine("Last connection time checker is alive!");
            _clientsCollection.CleanDeadClients(_clientHeartbeatPermittedGap);

            var networkEvents = await GetConnectionChangesAsNetworkEvents(_rtuHeartbeatPermittedGap);
            if (networkEvents.Count == 0)
                return 0;

            foreach (var networkEvent in networkEvents)
            {
                var command = new AddNetworkEvent()
                {
                    RtuId = networkEvent.RtuId,
                    EventTimestamp = DateTime.Now,
                    OnMainChannel = networkEvent.OnMainChannel,
                    OnReserveChannel = networkEvent.OnReserveChannel,
                };
                if (!string.IsNullOrEmpty(await _eventStoreService.SendCommand(command, "system", "OnServer")))
                    continue;
                // var lastEvent = _writeModel.NetworkEvents.LastOrDefault(n => n.RtuId == networkEvent.RtuId);
                // if (lastEvent == null) continue;

                var dto = Mapper.Map<NetworkEventDto>(networkEvent);
                await _ftSignalRClient.NotifyAll("AddNetworkEvent", dto.ToCamelCaseJson());

                var thread = new Thread(() => { NotifyAboutNewNetworkEvent(networkEvent); }) { IsBackground = true };
                thread.Start();
            }

            return 0;
        }

        private void NotifyAboutNewNetworkEvent(NetworkEvent networkEvent)
        {
            _snmpNotifier.SendRtuNetworkEvent(networkEvent);
            var isMainChannel = networkEvent.OnMainChannel != ChannelEvent.Nothing;
            var isOk = (isMainChannel ? networkEvent.OnMainChannel : networkEvent.OnReserveChannel) ==
                       ChannelEvent.Repaired;
            _smsManager.SendNetworkEvent(networkEvent.RtuId, isMainChannel, isOk);
            _smtp.SendNetworkEvent(networkEvent.RtuId, isMainChannel, isOk);
            _logFile.AppendLine("all notifications sent");
        }


        private async Task<List<NetworkEvent>> GetConnectionChangesAsNetworkEvents(TimeSpan rtuHeartbeatPermittedGap)
        {
            DateTime noLaterThan = DateTime.Now - rtuHeartbeatPermittedGap;
            var stations = await _rtuStationsRepository.GetAllRtuStations();

            List<RtuStation> changedStations = new List<RtuStation>();
            List<NetworkEvent> result = new List<NetworkEvent>();
            foreach (var rtuStation in stations)
            {
                NetworkEvent networkEvent = CheckRtuStation(rtuStation, noLaterThan);
                if (networkEvent != null)
                {
                    changedStations.Add(rtuStation);
                    result.Add(networkEvent);
                }
            }
            if (changedStations.Count > 0)
                await _rtuStationsRepository.SaveAvailabilityChanges(changedStations);

            return result;
        }

        private NetworkEvent CheckRtuStation(RtuStation rtuStation, DateTime noLaterThan)
        {
            var networkEvent = new NetworkEvent() { RtuId = rtuStation.RtuGuid, EventTimestamp = DateTime.Now };

            var flag = CheckMainChannel(rtuStation, noLaterThan, networkEvent);

            if (rtuStation.IsReserveAddressSet)
                flag = flag || CheckReserveChannel(rtuStation, noLaterThan, networkEvent);

            return flag ? networkEvent : null;
        }

        private bool CheckReserveChannel(RtuStation rtuStation, DateTime noLaterThan, NetworkEvent networkEvent)
        {
            var rtu = _writeModel.Rtus.FirstOrDefault(r => r.Id == rtuStation.RtuGuid);
            if (rtu == null) return false;
            var rtuTitle = rtu.Title;
            if (rtuStation.LastConnectionByReserveAddressTimestamp < noLaterThan &&
                rtuStation.IsReserveAddressOkDuePreviousCheck)
            {
                rtuStation.IsReserveAddressOkDuePreviousCheck = false;
                networkEvent.OnReserveChannel = ChannelEvent.Broken;
                _logFile.AppendLine($"RTU \"{rtuTitle}\" Reserve channel - Broken");
                return true;
            }

            if (rtuStation.LastConnectionByReserveAddressTimestamp >= noLaterThan &&
                !rtuStation.IsReserveAddressOkDuePreviousCheck)
            {
                rtuStation.IsReserveAddressOkDuePreviousCheck = true;
                networkEvent.OnReserveChannel = ChannelEvent.Repaired;
                _logFile.AppendLine($"RTU \"{rtuTitle}\" Reserve channel - Recovered");
                return true;
            }
            return false;
        }

        private bool CheckMainChannel(RtuStation rtuStation, DateTime noLaterThan, NetworkEvent networkEvent)
        {
            var rtu = _writeModel.Rtus.FirstOrDefault(r => r.Id == rtuStation.RtuGuid);
            if (rtu == null) return false;
            var rtuTitle = rtu.Title;
            if (rtuStation.LastConnectionByMainAddressTimestamp < noLaterThan && rtuStation.IsMainAddressOkDuePreviousCheck)
            {
                rtuStation.IsMainAddressOkDuePreviousCheck = false;
                networkEvent.OnMainChannel = ChannelEvent.Broken;
                _logFile.AppendLine($"RTU \"{rtuTitle}\" Main channel - Broken");
                return true;
            }

            if (rtuStation.LastConnectionByMainAddressTimestamp >= noLaterThan && !rtuStation.IsMainAddressOkDuePreviousCheck)
            {
                rtuStation.IsMainAddressOkDuePreviousCheck = true;
                networkEvent.OnMainChannel = ChannelEvent.Repaired;
                _logFile.AppendLine($"RTU \"{rtuTitle}\" Main channel - Recovered");

                return true;
            }
            return false;
        }


    }
}
