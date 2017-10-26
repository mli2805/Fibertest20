using System;
using System.Messaging;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.DataCenterService
{
    public class MsmqHandler
    {
        private readonly IniFile _iniFile;
        private readonly IMyLog _logFile;

        public MsmqHandler(IniFile iniFile, IMyLog logFile)
        {
            _iniFile = iniFile;
            _logFile = logFile;
        }

        public void Start()
        {
            var address = _iniFile.Read(IniSection.ServerMainAddress, IniKey.Ip, "192.168.96.0");
            var connectionString = $@"FormatName:DIRECT=TCP:{address}\private$\Fibertest20";
            var queue = new MessageQueue(connectionString);
            queue.ReceiveCompleted += MyReceiveCompleted;

            // Begin the asynchronous receive operation.
            queue.BeginReceive();
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

                _logFile.AppendLine($@"Message received, Body length = {message.BodyStream.Length}");
                var mr = message.Body as MonitoringResultDto;
                if (mr != null)
                {
                    _logFile.AppendLine($@"Monitoring result received, ID = {mr.Id}");
                    _logFile.AppendLine($"RTU {mr.RtuId.First6()} port {mr.OtauPort.OpticalPort} state {mr.TraceState}");
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
