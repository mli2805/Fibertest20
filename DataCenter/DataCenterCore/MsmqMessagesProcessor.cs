using System;
using System.Collections.Generic;
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
        private readonly Smtp _smtp;
        private readonly SmsManager _smsManager;

        public MsmqMessagesProcessor(IMyLog logFile, IniFile iniFile, Model writeModel,
            EventStoreService eventStoreService, MeasurementFactory measurementFactory,
            SorFileRepository sorFileRepository, RtuStationsRepository rtuStationsRepository,
            Smtp smtp, SmsManager smsManager)
        {
            _logFile = logFile;
            _iniFile = iniFile;
            _writeModel = writeModel;
            _eventStoreService = eventStoreService;
            _measurementFactory = measurementFactory;
            _sorFileRepository = sorFileRepository;
            _rtuStationsRepository = rtuStationsRepository;
            _smtp = smtp;
            _smsManager = smsManager;
        }

        public async Task<int> ProcessMessage(Message message)
        {
            if (message.Body is MonitoringResultDto monitoringResultDto)
                return await ProcessMonitoringResult(monitoringResultDto);
            if (message.Body is BopStateChangedDto bopStateChangedDto)
                return await ProcessBopStateChanges(bopStateChangedDto);
            return -1;

        }

        public async Task<int> ProcessBopStateChanges(BopStateChangedDto dto)
        {
            if (! await _rtuStationsRepository.IsRtuExist(dto.RtuId)) return -1;

            await CheckAndSendBopNetworkEventIfNeeded(dto);
            return 0;
        }

        public async Task<int> ProcessMultipleMonitoringResults(List<MonitoringResultDto> dtos)
        {
            _logFile.AppendLine("point1");
            if (! await _rtuStationsRepository.IsRtuExist(dtos.First().RtuId)) return -1;

            var sorIds = await _sorFileRepository.AddMultipleSorBytesAsync(dtos.Select(d=>d.SorBytes).ToList());
            _logFile.AppendLine("point2");
            if (sorIds == null) return -1;

            for (int i = 0; i < dtos.Count; i++)
            {
            _logFile.AppendLine("point3");
                var res = await SaveEventFromDto(dtos[i], sorIds[i]);
                if (res == -1)
                    _logFile.AppendLine("invalid moniresult");
            }
            _logFile.AppendLine("point4");

            return 0;
        }

        public async Task<int> ProcessMonitoringResult(MonitoringResultDto dto)
        {
            if (! await _rtuStationsRepository.IsRtuExist(dto.RtuId)) return -1;

            _logFile.AppendLine($@"MSMQ message, measure time: {dto.TimeStamp.ToString(Thread.CurrentThread.CurrentUICulture)}, RTU { dto.RtuId.First6()
                }, Trace {dto.PortWithTrace.TraceId.First6()} - {dto.TraceState} ({ dto.BaseRefType })");

            var sorId = await _sorFileRepository.AddSorBytesAsync(dto.SorBytes);
            if (sorId == -1) return -1;

            return await SaveEventFromDto(dto, sorId);
        }

        private async Task<int> SaveEventFromDto(MonitoringResultDto dto, int sorId)
        {
            _logFile.AppendLine("point11");
            var addMeasurement = _measurementFactory.CreateCommand(dto, sorId);
            _logFile.AppendLine("point12");
            var result = await _eventStoreService.SendCommand(addMeasurement, "system", "OnServer");
            _logFile.AppendLine("point13");

            if (result != null) // Unknown trace or something else
            {
                await _sorFileRepository.RemoveSorBytesAsync(sorId);
                return -1;
            }

            _logFile.AppendLine("point14");
            await CheckAndSendBopNetworkIfNeeded(dto);
            _logFile.AppendLine("point15");

            if (addMeasurement.EventStatus > EventStatus.JustMeasurementNotAnEvent && dto.BaseRefType != BaseRefType.Fast)
            {
                // ReSharper disable once UnusedVariable
                var task = Task.Factory.StartNew(() =>
                    SendNotificationsAboutTraces(dto, addMeasurement)); // here we do not wait result
            }
            _logFile.AppendLine("point16");

            return 0;
        }

        private async void SendNotificationsAboutTraces(MonitoringResultDto dto, AddMeasurement addMeasurement)
        {
            SetCulture();
           
            await _smtp.SendOpticalEvent(dto, addMeasurement);
            _smsManager.SendMonitoringResult(dto); 
        }

        private async void SendNotificationsAboutBop(AddBopNetworkEvent cmd)
        {
            SetCulture();
            await _smtp.SendBopState(cmd);
            _smsManager.SendBopState(cmd);
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
                _logFile.AppendLine($"RTU {dto.RtuId.First6()} BOP {dto.Serial} state changed to {dto.IsOk} (because MSMQ message about BOP came)");
                var cmd = new AddBopNetworkEvent()
                {
                    EventTimestamp = DateTime.Now,
                    RtuId = dto.RtuId,
                    Serial = dto.Serial,
                    OtauIp = otau.NetAddress.Ip4Address,
                    TcpPort = otau.NetAddress.Port,
                    IsOk = dto.IsOk,
                };
                await _eventStoreService.SendCommand(cmd, "system", "OnServer");
                // ReSharper disable once UnusedVariable
                var task = Task.Factory.StartNew(() => SendNotificationsAboutBop(cmd));
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
                    OtauIp = otau.NetAddress.Ip4Address,
                    TcpPort = otau.NetAddress.Port,
                    IsOk = true,
                };
                await _eventStoreService.SendCommand(cmd, "system", "OnServer");
                // ReSharper disable once UnusedVariable
                var task = Task.Factory.StartNew(() => SendNotificationsAboutBop(cmd));
            }
        }
    }
}