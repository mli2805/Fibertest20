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
        private readonly SmsManager _smsManager;
        private readonly GlobalState _globalState;

        public MsmqHandler(IniFile iniFile, IMyLog logFile, Model writeModel,
            SorFileRepository sorFileRepository, RtuStationsRepository rtuStationsRepository, 
            MeasurementFactory measurementFactory, EventStoreService eventStoreService,
            Smtp smtp, SmsManager smsManager
            ,GlobalState globalState)
        {
            _iniFile = iniFile;
            _logFile = logFile;
            _writeModel = writeModel;
            _sorFileRepository = sorFileRepository;
            _rtuStationsRepository = rtuStationsRepository;
            _measurementFactory = measurementFactory;
            _eventStoreService = eventStoreService;
            _smtp = smtp;
            _smsManager = smsManager;
            _globalState = globalState;
        }

        public void Start()
        {
            var address = _iniFile.Read(IniSection.ServerMainAddress, IniKey.Ip, "0.0.0.0");
            var connectionString = $@"FormatName:DIRECT=TCP:{address}\private$\Fibertest20";
            var queue = new MessageQueue(connectionString);
            queue.ReceiveCompleted += MyReceiveCompleted;

            // Begin the asynchronous receive operation.
            var tid = Thread.CurrentThread.ManagedThreadId;
            _logFile.AppendLine($"MSMQ {connectionString} listener started in thread {tid}");
            try
            {
                queue.BeginReceive();
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message + Environment.NewLine 
                       + "ini file should contain correct local IP (127.0.0.1 or localhost are NOT valid in this case)");
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

             //   await ProcessMessage(message);
                await Task.Factory.StartNew(() => ProcessMessage(message));
            }
            catch (Exception e)
            {
                _logFile.AppendLine($"MyReceiveCompleted: {e.Message}");
            }
            finally
            {
                // Restart the asynchronous receive operation.
                var tid = Thread.CurrentThread.ManagedThreadId;
                _logFile.AppendLine($"MSMQ restarted listener in thread {tid}");

                while (_globalState.IsDatacenterInDbOptimizationMode)
                {
                    Thread.Sleep(1000);
                }
               queue.BeginReceive();
            }
        }


        private async Task<int> ProcessMessage(Message message)
        {
            var tid = Thread.CurrentThread.ManagedThreadId;
            _logFile.AppendLine($"MSMQ processed message in thread {tid}");

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

        public async Task<int> ProcessMonitoringResult(MonitoringResultDto dto)
        {
            if (! await _rtuStationsRepository.IsRtuExist(dto.RtuId)) return -1;

            _logFile.AppendLine($@"MSMQ message, measure time: {dto.TimeStamp.ToString(Thread.CurrentThread.CurrentUICulture)}, RTU { dto.RtuId.First6()
                    }, Trace {dto.PortWithTrace.TraceId.First6()} - {dto.TraceState} ({ dto.BaseRefType })");

            var sorId = await _sorFileRepository.AddSorBytesAsync(dto.SorBytes);
            if (sorId == -1) return -1;

            var addMeasurement = _measurementFactory.CreateCommand(dto, sorId);
            var result = await _eventStoreService.SendCommand(addMeasurement, "system", "OnServer");

            if (result != null) // Unknown trace or something else
            {
                await _sorFileRepository.RemoveSorBytesAsync(sorId);
                return -1;
            }

            await CheckAndSendBopNetworkIfNeeded(dto);

            if (addMeasurement.EventStatus > EventStatus.JustMeasurementNotAnEvent && dto.BaseRefType != BaseRefType.Fast)
            {
                // ReSharper disable once UnusedVariable
                var task = Task.Factory.StartNew(() => SendNotificationsAboutTraces(dto, addMeasurement)); // here we do not wait result
            }

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
