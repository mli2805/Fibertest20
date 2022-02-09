using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using Iit.Fibertest.DatabaseLibrary;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;
using Newtonsoft.Json;

namespace Iit.Fibertest.DataCenterCore
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public partial class WcfServiceDesktopC2D : IWcfServiceDesktopC2D
    {
        private readonly EventStoreService _eventStoreService;
        private readonly MeasurementFactory _measurementFactory;
        private readonly ClientsCollection _clientsCollection;

        private readonly IniFile _iniFile;
        private readonly IMyLog _logFile;
        private readonly CurrentDatacenterParameters _currentDatacenterParameters;
        private readonly Model _writeModel;
        private readonly IEventStoreInitializer _eventStoreInitializer;

        private readonly RtuStationsRepository _rtuStationsRepository;
        private readonly IFtSignalRClient _ftSignalRClient;
        private readonly SorFileRepository _sorFileRepository;
        private readonly SnapshotRepository _snapshotRepository;
        private readonly BaseRefRepairmanIntermediary _baseRefRepairmanIntermediary;
        private readonly Smtp _smtp;
        private readonly SnmpAgent _snmpAgent;
        private readonly SmsManager _smsManager;
        private readonly DiskSpaceProvider _diskSpaceProvider;
        private readonly GlobalState _globalState;
        private readonly D2CWcfManager _d2CWcfManager;

        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.All
        };

        public WcfServiceDesktopC2D(IniFile iniFile, IMyLog logFile, CurrentDatacenterParameters currentDatacenterParameters,
            Model writeModel, IEventStoreInitializer eventStoreInitializer, EventStoreService eventStoreService,
            MeasurementFactory measurementFactory, ClientsCollection clientsCollection,
            RtuStationsRepository rtuStationsRepository, IFtSignalRClient ftSignalRClient,
            BaseRefRepairmanIntermediary baseRefRepairmanIntermediary,
            SorFileRepository sorFileRepository, SnapshotRepository snapshotRepository,
            Smtp smtp, SnmpAgent snmpAgent, SmsManager smsManager, DiskSpaceProvider diskSpaceProvider,
            GlobalState globalState, D2CWcfManager d2CWcfManager
            )
        {
            _iniFile = iniFile;
            _logFile = logFile;
            _currentDatacenterParameters = currentDatacenterParameters;
            _writeModel = writeModel;
            _eventStoreInitializer = eventStoreInitializer;
            _eventStoreService = eventStoreService;
            _measurementFactory = measurementFactory;
            _clientsCollection = clientsCollection;
            _rtuStationsRepository = rtuStationsRepository;
            _ftSignalRClient = ftSignalRClient;
            _sorFileRepository = sorFileRepository;
            _snapshotRepository = snapshotRepository;
            _baseRefRepairmanIntermediary = baseRefRepairmanIntermediary;
            _smtp = smtp;
            _snmpAgent = snmpAgent;
            _smsManager = smsManager;
            _diskSpaceProvider = diskSpaceProvider;
            _globalState = globalState;
            _d2CWcfManager = d2CWcfManager;

            var tid = Thread.CurrentThread.ManagedThreadId;
            _logFile.AppendLine($"Clients listener: works in thread {tid}");
        }

        public IWcfServiceDesktopC2D SetServerAddresses(DoubleAddress newServerAddress, string username, string clientIp)
        {
            // tests need this function exists 
            return this;
        }

        public async Task<bool> SendHeartbeat(HeartbeatDto dto)
        {
            await Task.Delay(1);
            return _clientsCollection.RegisterHeartbeat(dto.ConnectionId);
        }

        public async Task<bool> CheckServerConnection(CheckServerConnectionDto dto)
        {
            _logFile.AppendLine($"Client from {dto.ClientIp} checked server connection");
            await Task.Delay(1);
            return true;
        }

       
        public async Task<DiskSpaceDto> GetDiskSpaceGb()
        {
            return await _diskSpaceProvider.GetDiskSpaceGb();
        }

        private async Task<ClientStation> NotifyOptimizationStarted(string username, string clientIp)
        {
            if (_clientsCollection.Clients == null)
                return null;

            ClientStation result = null;
            foreach (var c in _clientsCollection.Clients)
            {
                if (c.ClientIp == clientIp && c.UserName == username)
                {
                    _d2CWcfManager.SetClientsAddresses(
                        new List<DoubleAddress>(){
                            new DoubleAddress() { Main = new NetAddress(c.ClientIp, c.ClientAddressPort) }
                        }
                    );
                    await _d2CWcfManager.BlockClientWhileDbOptimization(new DbOptimizationProgressDto() { Stage = DbOptimizationStage.Starting });
                    result = c;
                }
                else
                {
                    _d2CWcfManager.SetClientsAddresses(
                        new List<DoubleAddress>(){
                            new DoubleAddress() { Main = new NetAddress(c.ClientIp, c.ClientAddressPort) }
                        }
                    );
                    await _d2CWcfManager.ServerAsksClientToExit(new ServerAsksClientToExitDto()
                    {
                        ConnectionId = c.ConnectionId,
                        Reason = UnRegisterReason.DbOptimizationFinished,
                    });
                }
            }

            return result;
        }

    }
}
