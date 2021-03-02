using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
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

        public async Task<int> SendCommands(List<string> jsons, string username, string clientIp) // especially for Migrator.exe
        {
            var cmds = jsons.Select(json => JsonConvert.DeserializeObject(json, JsonSerializerSettings)).ToList();

            await _eventStoreService.SendCommands(cmds, username, clientIp);
            return jsons.Count;
        }

        public async Task<int> SendMeas(List<AddMeasurementFromOldBase> list)
        {
            foreach (var dto in list)
            {
                var sorId = await _sorFileRepository.AddSorBytesAsync(dto.SorBytes);
                if (sorId == -1) return -1;

                var command = _measurementFactory.CreateCommand(dto, sorId);
                await _eventStoreService.SendCommand(command, "migrator", "OnServer");
            }

            return 0;
        }

        public async Task<int> SendCommandsAsObjs(List<object> cmds)
        {
            // during the tests "client" invokes not the C2DWcfManager's method to communicate by network
            // but right server's method from WcfServiceForClient
            var username = "NCrunch";
            var clientIp = "127.0.0.1"; var list = new List<string>();

            foreach (var cmd in cmds)
            {
                list.Add(JsonConvert.SerializeObject(cmd, cmd.GetType(), JsonSerializerSettings));
            }

            return await SendCommands(list, username, clientIp);
        }

        public async Task<string> SendCommandAsObj(object cmd)
        {
            // during the tests "client" invokes not the C2DWcfManager's method to communicate by network
            // but right server's method from WcfServiceForClient
            var username = "NCrunch";
            var clientIp = "127.0.0.1";
            return await SendCommand(JsonConvert.SerializeObject(cmd, cmd.GetType(), JsonSerializerSettings), username, clientIp);
        }

        public async Task<string> SendCommand(string json, string username, string clientIp)
        {
            var cmd = JsonConvert.DeserializeObject(json, JsonSerializerSettings);

            if (cmd is RemoveEventsAndSors removeEventsAndSors)
            {
                await Task.Factory.StartNew(() => RemoveEventsAndSors(removeEventsAndSors, username, clientIp));
                return null;
            }

            if (cmd is MakeSnapshot makeSnapshot)
            {
                await Task.Factory.StartNew(() => MakeSnapshot(makeSnapshot, username, clientIp));
                return null;
            }

            if (cmd is CleanTrace cleanTrace) // only removes sor files, Trace will be cleaned further
            {
                var res = await RemoveSorFiles(cleanTrace.TraceId);
                if (!string.IsNullOrEmpty(res)) return res;
            }

            if (cmd is RemoveTrace removeTrace) // only removes sor files, Trace will be removed further
            {
                var res = await RemoveSorFiles(removeTrace.TraceId);
                if (!string.IsNullOrEmpty(res)) return res;
            }
         
            var resultInGraph = await _eventStoreService.SendCommand(cmd, username, clientIp);
            if (resultInGraph != null)
                return resultInGraph;

            // Some commands need to be reported to web client
            await NotifyWebClient(cmd);

            // A few commands need post-processing in Db or RTU
            return await PostProcessing(cmd);
        }

        private async Task<string> PostProcessing(object cmd)
        {
            if (cmd is RemoveRtu removeRtu)
                return await _rtuStationsRepository.RemoveRtuAsync(removeRtu.RtuId);

            #region Base ref amend
            if (cmd is UpdateAndMoveNode updateAndMoveNode)
                return await _baseRefRepairmanIntermediary.AmendForTracesWhichUseThisNode(updateAndMoveNode.NodeId);
            if (cmd is UpdateRtu updateRtu)
                return await _baseRefRepairmanIntermediary.AmendForTracesFromRtu(updateRtu.RtuId);
            if (cmd is UpdateNode updateNode)
                return await _baseRefRepairmanIntermediary.AmendForTracesWhichUseThisNode(updateNode.NodeId);
            if (cmd is MoveNode moveNode)
                return await _baseRefRepairmanIntermediary.AmendForTracesWhichUseThisNode(moveNode.NodeId);
            if (cmd is UpdateEquipment updateEquipment)
                return await _baseRefRepairmanIntermediary.ProcessUpdateEquipment(updateEquipment.EquipmentId);
            if (cmd is UpdateFiber updateFiber)
                return await _baseRefRepairmanIntermediary.ProcessUpdateFiber(updateFiber.Id);
            if (cmd is AddNodeIntoFiber addNodeIntoFiber)
                return await _baseRefRepairmanIntermediary.AmendForTracesWhichUseThisNode(addNodeIntoFiber.Id);
            if (cmd is RemoveNode removeNode && removeNode.IsAdjustmentPoint)
                return await _baseRefRepairmanIntermediary.ProcessNodeRemoved(removeNode.DetoursForGraph.Select(d => d.TraceId)
                    .ToList());
            #endregion

            return null;
        }

        private static readonly IMapper Mapper = new MapperConfiguration(
            cfg => cfg.AddProfile<MappingWebApiProfile>()).CreateMapper();
        private async Task NotifyWebClient(object cmd)
        {
            switch (cmd)
            {
                case UpdateMeasurement _:
                    {
                        var evnt = Mapper.Map<UpdateMeasurementDto>(cmd);
                        await _ftSignalRClient.NotifyAll("UpdateMeasurement", evnt.ToCamelCaseJson());
                        break;
                    }

                case AddRtuAtGpsLocation _:
                case RemoveRtu _:
                case AttachTrace _:
                case DetachTrace _:
                case DetachAllTraces _:
                case CleanTrace _:
                case RemoveTrace _:
                case AddTrace _:
                    {
                        await _ftSignalRClient.NotifyAll("FetchTree", null);
                        break;
                    }
            }
        }

        public async Task<string[]> GetEvents(GetEventsDto dto)
        {
            if (!_clientsCollection.RegisterHeartbeat(dto.ConnectionId)) return null;
            return await Task.FromResult(_eventStoreService.GetEvents(dto.Revision));
        }

        public async Task<SnapshotParamsDto> GetSnapshotParams(GetSnapshotDto dto)
        {
            _clientsCollection.RegisterHeartbeat(dto.ConnectionId);
            return await _snapshotRepository.GetSnapshotParams(dto.LastIncludedEvent);
        }

        public async Task<byte[]> GetSnapshotPortion(int portionOrdinal)
        {
            return await _snapshotRepository.GetSnapshotPortion(portionOrdinal);
        }


        public async Task<DiskSpaceDto> GetDiskSpaceGb()
        {
            return await _diskSpaceProvider.GetDiskSpaceGb();
        }

        private async Task<string> RemoveSorFiles(Guid traceId)
        {
            var trace = _writeModel.Traces.FirstOrDefault(t => t.TraceId == traceId);
            if (trace == null)
                return $@"Trace {traceId} not found";

            // starting in another thread breaks tests
            // await Task.Factory.StartNew(() => LongPart(traceId, cleanTrace, username, clientIp));

            return await LongPart(traceId);
        }

        private async Task<string> LongPart(Guid traceId)
        {
            var sorFileIds = _writeModel.Measurements
                .Where(m => m.TraceId == traceId)
                .Select(l => l.SorFileId).ToArray();
            var sorFileIds2 = sorFileIds.Concat(_writeModel.BaseRefs
                .Where(b => b.TraceId == traceId)
                .Select(l => l.SorFileId).ToArray())
                .ToArray();
            await _sorFileRepository.RemoveManySorAsync(sorFileIds2);
            // return await _eventStoreService.SendCommand(cmd, username, clientIp);
            return null;
        }
    }
}
