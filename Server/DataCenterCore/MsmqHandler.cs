using System;
using System.Linq;
using System.Messaging;
using System.Threading.Tasks;
using Iit.Fibertest.DatabaseLibrary;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.DataCenterCore
{
    public class MsmqHandler
    {
        private readonly IniFile _iniFile;
        private readonly IMyLog _logFile;
        private readonly SorFileRepository _sorFileRepository;
        private readonly RtuStationsRepository _rtuStationsRepository;
        private readonly MeasurementFactory _measurementFactory;
        private readonly EventStoreService _eventStoreService;

        public MsmqHandler(IniFile iniFile, IMyLog logFile,
            SorFileRepository sorFileRepository, RtuStationsRepository rtuStationsRepository, MeasurementFactory measurementFactory,
            EventStoreService eventStoreService)
        {
            _iniFile = iniFile;
            _logFile = logFile;
            _sorFileRepository = sorFileRepository;
            _rtuStationsRepository = rtuStationsRepository;
            _measurementFactory = measurementFactory;
            _eventStoreService = eventStoreService;
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
            if (!(message.Body is MonitoringResultDto monitoringResultDto))
                return -1;

            var rtus = await _rtuStationsRepository.GetAllRtuStations();
            if (rtus.FirstOrDefault(r => r.RtuGuid == monitoringResultDto.RtuId) == null)
            {
                _logFile.AppendLine($"Unknown RTU {monitoringResultDto.RtuId.First6()}");
                return -1;
            }

            _logFile.AppendLine($@"MSMQ message, measure time: {monitoringResultDto.TimeStamp:dd-MM-yyyy hh:mm:ss}, RTU {monitoringResultDto.RtuId.First6()}, Trace {monitoringResultDto.PortWithTrace.TraceId.First6()} - {monitoringResultDto.TraceState} ({monitoringResultDto.BaseRefType})");

            var sorId = await _sorFileRepository.AddSorBytesAsync(monitoringResultDto.SorBytes);
            if (sorId == -1) return -1;

            var command = _measurementFactory.CreateCommand(monitoringResultDto, sorId);
            await _eventStoreService.SendCommand(command, "system", "OnServer");

            // TODO snmp, email, sms
            if (command.EventStatus > EventStatus.JustMeasurementNotAnEvent)
            {

            }
            return 0;
        }

    }
}
