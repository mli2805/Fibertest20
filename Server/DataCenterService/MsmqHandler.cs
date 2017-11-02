﻿using System;
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
        private readonly DcManager _dcManager;

        public MsmqHandler(IniFile iniFile, IMyLog logFile, DcManager dcManager)
        {
            _iniFile = iniFile;
            _logFile = logFile;
            _dcManager = dcManager;
        }

        public void Start()
        {
            var connectionString = @"FormatName:DIRECT=TCP:127.0.0.1\private$\Fibertest20";
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
                    _dcManager.ProcessMonitoringResult(mr);
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
