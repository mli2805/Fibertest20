using System;
using System.Globalization;
using System.Linq;
using System.Messaging;
using System.Threading;
using System.Threading.Tasks;
using Iit.Fibertest.DatabaseLibrary;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.DataCenterCore
{
    public class MsmqMessagesProcessor
    {
        private readonly IMyLog _logFile;
        private readonly IniFile _iniFile;
        private readonly Model _writeModel;
        private readonly EventStoreService _eventStoreService;
        private readonly MeasurementFactory _measurementFactory;
        private readonly SorFileRepository _sorFileRepository;
        private readonly RtuStationsRepository _rtuStationsRepository;
        private readonly FtSignalRClient _ftSignalRClient;
        private readonly AccidentLineModelFactory _accidentLineModelFactory;
        private readonly Smtp _smtp;
        private readonly SmsManager _smsManager;
        private readonly SnmpNotifier _snmpNotifier;

        public MsmqMessagesProcessor(IMyLog logFile, IniFile iniFile, Model writeModel,
            EventStoreService eventStoreService, MeasurementFactory measurementFactory,
            SorFileRepository sorFileRepository, RtuStationsRepository rtuStationsRepository,
            FtSignalRClient ftSignalRClient, AccidentLineModelFactory accidentLineModelFactory,
            Smtp smtp, SmsManager smsManager, SnmpNotifier snmpNotifier)
        {
            _logFile = logFile;
            _iniFile = iniFile;
            _writeModel = writeModel;
            _eventStoreService = eventStoreService;
            _measurementFactory = measurementFactory;
            _sorFileRepository = sorFileRepository;
            _rtuStationsRepository = rtuStationsRepository;
            _ftSignalRClient = ftSignalRClient;
            _accidentLineModelFactory = accidentLineModelFactory;
            _smtp = smtp;
            _smsManager = smsManager;
            _snmpNotifier = snmpNotifier;
        }

        public async Task ProcessMessage(Message message)
        {
            if (message.Body is MonitoringResultDto monitoringResultDto)
                await ProcessMonitoringResult(monitoringResultDto);
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

            _logFile.AppendLine($@"MSMQ message, measure time: {dto.TimeStamp.ToString(Thread.CurrentThread.CurrentUICulture)
                }, RTU { dto.RtuId.First6()
                }, Trace {dto.PortWithTrace.TraceId.First6()} - {dto.TraceState} ({ dto.BaseRefType })");

            var sorId = await _sorFileRepository.AddSorBytesAsync(dto.SorBytes);
            if (sorId != -1) 
                await SaveEventFromDto(dto, sorId);
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

        private async void SendNotificationsAboutTraces(MonitoringResultDto dto, AddMeasurement addMeasurement)
        {
            SetCulture();

            await _smtp.SendOpticalEvent(dto, addMeasurement);
            _smsManager.SendMonitoringResult(dto);
            _snmpNotifier.SendTraceEvent(addMeasurement);
        }

        private async void SendNotificationsAboutBop(AddBopNetworkEvent cmd)
        {
            SetCulture();
            await _smtp.SendBopState(cmd);
            _smsManager.SendBopState(cmd);
            _snmpNotifier.SendBopNetworkEvent(cmd);
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
                o.Serial == dto.Serial
            );
            if (otau != null)
            {
                _logFile.AppendLine($@"RTU {dto.RtuId.First6()} BOP {dto.Serial} state changed to {
                    dto.IsOk} (because MSMQ message about BOP came)");
                var cmd = new AddBopNetworkEvent()
                {
                    EventTimestamp = DateTime.Now,
                    RtuId = dto.RtuId,
                    Serial = dto.Serial,
                    OtauIp = otau.OtauAddress.Ip4Address,
                    TcpPort = otau.OtauAddress.Port,
                    IsOk = dto.IsOk,
                };
                await _eventStoreService.SendCommand(cmd, "system", "OnServer");
                var unused = Task.Factory.StartNew(() => SendNotificationsAboutBop(cmd));
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
                _logFile.AppendLine($"RTU {dto.RtuId.First6()} BOP {dto.PortWithTrace.OtauPort.Serial} state changed to OK (because MSMQ message with monitoring result came)");
                var cmd = new AddBopNetworkEvent()
                {
                    EventTimestamp = DateTime.Now,
                    RtuId = dto.RtuId,
                    Serial = dto.PortWithTrace.OtauPort.Serial,
                    OtauIp = otau.OtauAddress.Ip4Address,
                    TcpPort = otau.OtauAddress.Port,
                    IsOk = true,
                };
                await _eventStoreService.SendCommand(cmd, "system", "OnServer");
                var unused = Task.Factory.StartNew(() => SendNotificationsAboutBop(cmd));
            }
        }
    }
}