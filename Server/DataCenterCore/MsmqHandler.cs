using System;
using System.Messaging;
using System.Threading.Tasks;
using Iit.Fibertest.DatabaseLibrary;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;

namespace Iit.Fibertest.DataCenterCore
{
    public class MsmqHandler
    {
        private readonly IniFile _iniFile;
        private readonly IMyLog _logFile;
        private readonly MonitoringResultsRepository _monitoringResultsRepository;
        private readonly MeasurementFactory _measurementFactory;
        private readonly ClientStationsRepository _clientStationsRepository;
        private readonly D2CWcfManager _d2CWcfManager;

        public MsmqHandler(IniFile iniFile, IMyLog logFile, 
            MonitoringResultsRepository monitoringResultsRepository, MeasurementFactory measurementFactory,
            ClientStationsRepository clientStationsRepository,
            D2CWcfManager d2CWcfManager)
        {
            _iniFile = iniFile;
            _logFile = logFile;
            _monitoringResultsRepository = monitoringResultsRepository;
            _measurementFactory = measurementFactory;
            _clientStationsRepository = clientStationsRepository;
            _d2CWcfManager = d2CWcfManager;
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

        private void MyReceiveCompleted(object source, ReceiveCompletedEventArgs asyncResult)
        {
            // Connect to the queue.
            MessageQueue queue = (MessageQueue)source;
            try
            {
                queue.Formatter = new BinaryMessageFormatter();

                // End the asynchronous receive operation.
                Message message = queue.EndReceive(asyncResult.AsyncResult);

                ProcessMessage(message);
           
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


        private async void ProcessMessage(Message message)
        {
            if (!(message.Body is MonitoringResultDto monitoringResultDto))
                return;

            _logFile.AppendLine($@"MSMQ message received, RTU {monitoringResultDto.RtuId.First6()}, 
                        Trace {monitoringResultDto.PortWithTrace.TraceId.First6()} - {monitoringResultDto.TraceState} ({monitoringResultDto.BaseRefType})");
            var measurementWithSor = await _monitoringResultsRepository.
                SaveMonitoringResultAsync(monitoringResultDto.SorData, _measurementFactory.Create(monitoringResultDto));

            if (measurementWithSor != null)
            {
                await SendMoniresultToClients(measurementWithSor);

                // TODO snmp, email, sms
                if (measurementWithSor.Measurement.EventStatus > EventStatus.JustMeasurementNotAnEvent)
                {

                }
            }
        }

        private async Task<int> SendMoniresultToClients(MeasurementWithSor measurementWithSor)
        {
            var addresses = await _clientStationsRepository.GetClientsAddresses();
            if (addresses == null)
                return 0;
            _d2CWcfManager.SetClientsAddresses(addresses);
            return await _d2CWcfManager.NotifyAboutMonitoringResult(measurementWithSor);
        }
    }
}
