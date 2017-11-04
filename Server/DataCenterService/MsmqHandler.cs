using System;
using System.Messaging;
using Iit.Fibertest.DataCenterCore;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.DataCenterService
{
    public class MsmqHandler
    {
        private readonly IniFile _iniFile;
        private readonly IMyLog _logFile;
        private readonly MonitoringResultsManager _monitoringResultsManager;

        public MsmqHandler(IniFile iniFile, IMyLog logFile, MonitoringResultsManager monitoringResultsManager)
        {
            _iniFile = iniFile;
            _logFile = logFile;
            _monitoringResultsManager = monitoringResultsManager;
        }

        public void Start()
        {
            // ini file should contain correct local ip (127.0.0.1 or localhost are NOT valid in this case)
            var address = _iniFile.Read(IniSection.ServerMainAddress, IniKey.Ip, "192.168.96.0");
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
            try
            {
                // Connect to the queue.
                MessageQueue queue = (MessageQueue)source;
                queue.Formatter = new BinaryMessageFormatter();

                // End the asynchronous receive operation.
                Message message = queue.EndReceive(asyncResult.AsyncResult);

                _logFile.AppendLine($@"MSMQ message received, Body length = {message.BodyStream.Length}");

                var mr = message.Body as MonitoringResultDto;
                if (mr != null)
                {
                    _monitoringResultsManager.ProcessMonitoringResult(mr);
                }

                // Restart the asynchronous receive operation.
                queue.BeginReceive();
            }
            catch (Exception e)
            {
                _logFile.AppendLine($"MyReceiveCompleted: {e.Message}");
            }
        }
    }
}
