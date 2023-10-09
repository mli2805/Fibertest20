using System;
using System.Globalization;
using System.Linq;
using System.Messaging;
using System.Threading;
using System.Threading.Tasks;
using Iit.Fibertest.DatabaseLibrary;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.RtuOccupy;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.DataCenterCore
{
    public class MsmqMessagesProcessor
    {
        private readonly IMyLog _logFile;
        private readonly IniFile _iniFile;
        private readonly Model _writeModel;
        private readonly CommonBopProcessor _commonBopProcessor;
        private readonly RtuOccupations _rtuOccupations;
        private readonly EventStoreService _eventStoreService;
        private readonly MeasurementFactory _measurementFactory;
        private readonly SorFileRepository _sorFileRepository;
        private readonly RtuStationsRepository _rtuStationsRepository;
        private readonly IFtSignalRClient _ftSignalRClient;
        private readonly AccidentLineModelFactory _accidentLineModelFactory;
        private readonly Smtp _smtp;
        private readonly SmsManager _smsManager;
        private readonly SnmpNotifier _snmpNotifier;

        private readonly string _trapSenderUser;

        public MsmqMessagesProcessor(IMyLog logFile, IniFile iniFile, Model writeModel,
            CommonBopProcessor commonBopProcessor, RtuOccupations rtuOccupations,
            EventStoreService eventStoreService, MeasurementFactory measurementFactory,
            SorFileRepository sorFileRepository, RtuStationsRepository rtuStationsRepository,
            IFtSignalRClient ftSignalRClient, AccidentLineModelFactory accidentLineModelFactory,
            Smtp smtp, SmsManager smsManager, SnmpNotifier snmpNotifier)
        {
            _logFile = logFile;
            _iniFile = iniFile;
            _writeModel = writeModel;
            _commonBopProcessor = commonBopProcessor;
            _rtuOccupations = rtuOccupations;
            _eventStoreService = eventStoreService;
            _measurementFactory = measurementFactory;
            _sorFileRepository = sorFileRepository;
            _rtuStationsRepository = rtuStationsRepository;
            _ftSignalRClient = ftSignalRClient;
            _accidentLineModelFactory = accidentLineModelFactory;
            _smtp = smtp;
            _smsManager = smsManager;
            _snmpNotifier = snmpNotifier;

            _trapSenderUser = rtuOccupations.ServerNameForTraps;
        }

        public async Task ProcessMessage(Message message)
        {
            if (message.Body is MonitoringResultDto dto)
            {
                var measResult = dto.ReturnCode == ReturnCode.MeasurementEndedNormally
                    ? dto.TraceState.ToString()
                    : dto.ReturnCode.ToString();

                var rtuStr = $"{_writeModel.Rtus.FirstOrDefault(r => r.Id == dto.RtuId)?.Title ?? $"not found {dto.RtuId.First6()}"}";
                var portStr = $"{dto.PortWithTrace.OtauPort.ToStringB()}";
                var traceNotFound = $"not found {dto.PortWithTrace.TraceId.First6()}";
                var traceStr = $" {_writeModel.Traces.FirstOrDefault(t => t.TraceId == dto.PortWithTrace.TraceId)?.Title ?? traceNotFound}";

                _logFile.AppendLine($@"MSMQ, measured: {dto.TimeStamp.ToString(Thread.CurrentThread.CurrentUICulture)}, {rtuStr}, Port {portStr}, {traceStr} - {measResult} ({dto.BaseRefType})");

                await ProcessMonitoringResult(dto);
            }
            if (message.Body is BopStateChangedDto bopStateChangedDto)
                await ProcessBopStateChanges(bopStateChangedDto);
        }

        public async Task ProcessBopStateChanges(BopStateChangedDto dto)
        {
            if (await _rtuStationsRepository.IsRtuExist(dto.RtuId))
                await CheckAndSendBopNetworkEventIfNeeded(dto);
        }

        public async Task ProcessMonitoringResult(MonitoringResultDto dto)
        {
            if (!await _rtuStationsRepository.IsRtuExist(dto.RtuId)) return;

            _rtuOccupations.TrySetOccupation(dto.RtuId, RtuOccupation.None, _trapSenderUser, out RtuOccupationState _);

            if ((dto.Reason ^ ReasonToSendMonitoringResult.MeasurementAccidentStatusChanged) != 0)
            {
                var sorId = await _sorFileRepository.AddSorBytesAsync(dto.SorBytes);
                if (sorId != -1)
                    await SaveEventFromDto(dto, sorId);
            }

            if ((dto.Reason & ReasonToSendMonitoringResult.MeasurementAccidentStatusChanged) != 0)
            {
                // if dto.ReturnCode != ReturnCode.MeasurementEndedNormally - it is an accident
                // if dto.ReturnCode == ReturnCode.MeasurementEndedNormally - restored after accident
                var accident = await SaveRtuAccident(dto);
                var unused = Task.Factory.StartNew(() => SendNotificationsAboutRtuStatusEvents(accident));
            }

        }

        private async Task<RtuAccident> SaveRtuAccident(MonitoringResultDto dto)
        {
            var addRtuAccident = _measurementFactory.CreateRtuAccidentCommand(dto);
            var result = await _eventStoreService.SendCommand(addRtuAccident, "system", "OnServer");
            if (result != null)
                _logFile.AppendLine($"SaveRtuAccident {result}");

            var accident = _writeModel.RtuAccidents.Last();
            await _ftSignalRClient.NotifyAll("AddAccident", accident.ToCamelCaseJson());
            return accident;
        }

        private async Task SaveEventFromDto(MonitoringResultDto dto, int sorId)
        {
            var addMeasurement = _measurementFactory.CreateCommand(dto, sorId);
            var result = await _eventStoreService.SendCommand(addMeasurement, "system", "OnServer");

            if (result != null) // Unknown trace or something else
            {
                await _sorFileRepository.RemoveSorBytesAsync(sorId);
                return;
            }

            var signal = _writeModel.GetTraceStateDto(_accidentLineModelFactory, sorId);
            await _ftSignalRClient.NotifyAll("AddMeasurement", signal.ToCamelCaseJson());
            await CheckAndSendBopNetworkIfNeeded(dto);

            if (addMeasurement.EventStatus > EventStatus.JustMeasurementNotAnEvent && dto.BaseRefType != BaseRefType.Fast)
            {
                var unused = Task.Factory.StartNew(() =>
                    SendNotificationsAboutTraces(dto, addMeasurement)); // here we do not wait result
            }
        }

        private async Task SendNotificationsAboutTraces(MonitoringResultDto dto, AddMeasurement addMeasurement)
        {
            SetCulture();

            await _smtp.SendOpticalEvent(dto, addMeasurement);
            _smsManager.SendMonitoringResult(dto);
            _snmpNotifier.SendTraceEvent(addMeasurement);
        }

        private async Task SendNotificationsAboutRtuStatusEvents(RtuAccident accident)
        {
            await Task.Delay(0);
            _snmpNotifier.SendRtuStatusEvent(accident);
        }

        private void SetCulture()
        {
            var cu = _iniFile.Read(IniSection.General, IniKey.Culture, "ru-RU");
            var currentCulture = new CultureInfo(cu);
            Thread.CurrentThread.CurrentUICulture = currentCulture;
        }

        // BOP - because MSMQ message about BOP came
        private async Task CheckAndSendBopNetworkEventIfNeeded(BopStateChangedDto dto)
        {
            var otau = _writeModel.Otaus.FirstOrDefault(o =>
                o.NetAddress.Ip4Address == dto.OtauIp && o.NetAddress.Port == dto.TcpPort
            );
            if (otau != null)
            {
                _logFile.AppendLine($@"RTU {dto.RtuId.First6()} BOP {otau.NetAddress.ToStringA()} state changed to {dto.IsOk} (because MSMQ message about BOP came)");
                var cmd = new AddBopNetworkEvent()
                {
                    EventTimestamp = DateTime.Now,
                    RtuId = dto.RtuId,
                    Serial = dto.Serial,
                    OtauIp = otau.NetAddress.Ip4Address,
                    TcpPort = otau.NetAddress.Port,
                    IsOk = dto.IsOk,
                };
                await _commonBopProcessor.PersistBopEvent(cmd);
            }
        }

        // BOP - because MSMQ message with monitoring result came
        private async Task CheckAndSendBopNetworkIfNeeded(MonitoringResultDto dto)
        {
            var otau = _writeModel.Otaus.FirstOrDefault(o =>
                o.Serial == dto.PortWithTrace.OtauPort.Serial
            );
            if (otau != null && !otau.IsOk)
            {
                _logFile.AppendLine($@"RTU {dto.RtuId.First6()} BOP {dto.PortWithTrace.OtauPort.Serial} state changed to OK (because MSMQ message with monitoring result came)");
                var cmd = new AddBopNetworkEvent()
                {
                    EventTimestamp = DateTime.Now,
                    RtuId = dto.RtuId,
                    Serial = dto.PortWithTrace.OtauPort.Serial,
                    OtauIp = otau.NetAddress.Ip4Address,
                    TcpPort = otau.NetAddress.Port,
                    IsOk = true,
                };
                await _commonBopProcessor.PersistBopEvent(cmd);
            }
        }
    }
}