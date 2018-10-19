using System;
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
    public class MsmqHandler
    {
        private readonly IniFile _iniFile;
        private readonly IMyLog _logFile;
        private readonly Model _writeModel;
        private readonly SorFileRepository _sorFileRepository;
        private readonly RtuStationsRepository _rtuStationsRepository;
        private readonly MeasurementFactory _measurementFactory;
        private readonly EventStoreService _eventStoreService;
        private readonly Smtp _smtp;
        private readonly Sms _sms;

        public MsmqHandler(IniFile iniFile, IMyLog logFile, Model writeModel,
            SorFileRepository sorFileRepository, RtuStationsRepository rtuStationsRepository, 
            MeasurementFactory measurementFactory, EventStoreService eventStoreService,
            Smtp smtp, Sms sms)
        {
            _iniFile = iniFile;
            _logFile = logFile;
            _writeModel = writeModel;
            _sorFileRepository = sorFileRepository;
            _rtuStationsRepository = rtuStationsRepository;
            _measurementFactory = measurementFactory;
            _eventStoreService = eventStoreService;
            _smtp = smtp;
            _sms = sms;
        }

        public void Start()
        {
            // ini file should contain correct local ip (127.0.0.1 or localhost are NOT valid in this case)
            var address = _iniFile.Read(IniSection.ServerMainAddress, IniKey.Ip, "0.0.0.0");
            var connectionString = $@"FormatName:DIRECT=TCP:{address}\private$\Fibertest20";
            var queue = new MessageQueue(connectionString);
            queue.ReceiveCompleted += MyReceiveCompleted;

            // Begin the asynchronous receive operation.
            _logFile.AppendLine($"MSMQ {connectionString} listener started");
            try
            {
                queue.BeginReceive();
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                throw;
            }
        }

        private async void MyReceiveCompleted(object source, ReceiveCompletedEventArgs asyncResult)
        {
            // Connect to the queue.
            MessageQueue queue = (MessageQueue)source;
            try
            {
                queue.Formatter = new BinaryMessageFormatter();

                // End the asynchronous receive operation.
                Message message = queue.EndReceive(asyncResult.AsyncResult);

                await ProcessMessage(message);

            }
            catch (Exception e)
            {
                _logFile.AppendLine($"MyReceiveCompleted: {e.Message}");
            }
            finally
            {
                // Restart the asynchronous receive operation.
                queue.BeginReceive();
            }
        }


        private async Task<int> ProcessMessage(Message message)
        {
            if (message.Body is MonitoringResultDto monitoringResultDto)
                return await ProcessMonitoringResult(monitoringResultDto);
            if (message.Body is BopStateChangedDto bopStateChangedDto)
                return await ProcessBopStateChanges(bopStateChangedDto);
            return -1;

        }

        public async Task<int> ProcessBopStateChanges(BopStateChangedDto dto)
        {
            var rtus = await _rtuStationsRepository.GetAllRtuStations();
            if (rtus.FirstOrDefault(r => r.RtuGuid == dto.RtuId) == null)
            {
                _logFile.AppendLine($"Unknown RTU {dto.RtuId.First6()}");
                return -1;
            }

            await CheckAndSendBopNetworkEventIfNeeded(dto);
            return 0;
        }

        public async Task<int> ProcessMonitoringResult(MonitoringResultDto dto)
        {
            var rtus = await _rtuStationsRepository.GetAllRtuStations();
            if (rtus.FirstOrDefault(r => r.RtuGuid == dto.RtuId) == null)
            {
                _logFile.AppendLine($"Unknown RTU {dto.RtuId.First6()}");
                return -1;
            }

            _logFile.AppendLine($@"MSMQ message, measure time: {dto.TimeStamp.ToString(Thread.CurrentThread.CurrentUICulture)}, RTU { dto.RtuId.First6()
                    }, Trace {dto.PortWithTrace.TraceId.First6()} - {dto.TraceState} ({ dto.BaseRefType })");

            var sorId = await _sorFileRepository.AddSorBytesAsync(dto.SorBytes);
            if (sorId == -1) return -1;

            var command = _measurementFactory.CreateCommand(dto, sorId);
            var result = await _eventStoreService.SendCommand(command, "system", "OnServer");

            if (result != null) // Unknown trace or something else
            {
                await _sorFileRepository.RemoveSorBytesAsync(sorId);
                return -1;
            }

            await CheckAndSendBopNetworkIfNeeded(dto);

            if (command.EventStatus > EventStatus.JustMeasurementNotAnEvent && dto.BaseRefType != BaseRefType.Fast)
            {
                // ReSharper disable once UnusedVariable
                var task = Task.Factory.StartNew(() => SendNotifications(dto));
            }

            return 0;
        }

            // TODO SNMP, SMTP, SMS
        private async void SendNotifications(MonitoringResultDto dto)
        {
            await _smtp.SendMonitoringResult(dto);
            await _sms.SendMonitoringResult(dto); 
        }

        // BOP - because MSMQ message came
        private async Task CheckAndSendBopNetworkEventIfNeeded(BopStateChangedDto dto)
        {
            if (_writeModel.Otaus.Any(o =>
                o.NetAddress.Ip4Address == dto.OtauIp
                && o.NetAddress.Port == dto.TcpPort
                && o.IsOk != dto.IsOk))
            {
                _logFile.AppendLine($"RTU {dto.RtuId.First6()} BOP {dto.OtauIp} state changed to {dto.IsOk} (because MSMQ message came)");
                var cmd = new AddBopNetworkEvent()
                {
                    EventTimestamp = DateTime.Now,
                    RtuId = dto.RtuId,
                    OtauIp = dto.OtauIp,
                    TcpPort = dto.TcpPort,
                    IsOk = dto.IsOk,
                };
                await _eventStoreService.SendCommand(cmd, "system", "OnServer");
            }
        }

        // BOP - because monitoring result came
        private async Task CheckAndSendBopNetworkIfNeeded(MonitoringResultDto dto)
        {
            if (_writeModel.Otaus.Any(o =>
                o.NetAddress.Ip4Address == dto.PortWithTrace.OtauPort.OtauIp
                && o.NetAddress.Port == dto.PortWithTrace.OtauPort.OtauTcpPort
                && !o.IsOk))
            {
                _logFile.AppendLine($"RTU {dto.RtuId.First6()} BOP {dto.PortWithTrace.OtauPort.OtauIp} state changed to OK (because monitoring result came)");
                var cmd = new AddBopNetworkEvent()
                {
                    EventTimestamp = DateTime.Now,
                    RtuId = dto.RtuId,
                    OtauIp = dto.PortWithTrace.OtauPort.OtauIp,
                    TcpPort = dto.PortWithTrace.OtauPort.OtauTcpPort,
                    IsOk = true,
                };
                await _eventStoreService.SendCommand(cmd, "system", "OnServer");
            }
        }
    }
}
