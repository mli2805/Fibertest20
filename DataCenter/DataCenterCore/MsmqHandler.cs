using System;
using System.Collections.Generic;
using System.Messaging;
using System.Threading;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.DataCenterCore
{
    public interface IMsmqHandler
    {
        void Start();

    }
    public class MsmqHandler2 : IMsmqHandler
    {
        private readonly IniFile _iniFile;
        private readonly IMyLog _logFile;
        private readonly MsmqMessagesProcessor _msmqMessagesProcessor;


        public MsmqHandler2(IniFile iniFile, IMyLog logFile, MsmqMessagesProcessor msmqMessagesProcessor)
        {
            _iniFile = iniFile;
            _logFile = logFile;
            _msmqMessagesProcessor = msmqMessagesProcessor;
        }

        public void Start()
        {
            var address = _iniFile.Read(IniSection.ServerMainAddress, IniKey.Ip, "0.0.0.0");
            var connectionString = $@"FormatName:DIRECT=TCP:{address}\private$\Fibertest20";
            var tid = Thread.CurrentThread.ManagedThreadId;
            _logFile.AppendLine($"MSMQ {connectionString} listener started in thread {tid}");
            var queue = new MessageQueue(connectionString);

            Task.Factory.StartNew(() => F(queue));
        }

        private async Task<int> F(MessageQueue queue)
        {
            while (true)
            {
                try
                {
                    while (true)
                    {
                        var unused = await GetPortion(queue);
                        await Task.Delay(1000);
                    }
                }
                catch (Exception e)
                {
                    if (e.Message == "Sequence contains no elements")
                        continue;
                    _logFile.AppendLine(e.Message + Environment.NewLine
                                                  + "ini file should contain correct local IP (127.0.0.1 or localhost are NOT valid in this case)");
                }
            }
            // ReSharper disable once FunctionNeverReturns
        }

        private async Task<int> GetPortion(MessageQueue queue)
        {
                queue.Formatter = new BinaryMessageFormatter();
            var portion = 0;
            var msgEnumerator = queue.GetMessageEnumerator2();
            var moniResults = new List<MonitoringResultDto>();
            while (msgEnumerator.MoveNext() && portion < 10)
            {
                if (msgEnumerator.Current != null)
                {
                  //  var message = queue.ReceiveById(msgEnumerator.Current.Id);
                    var message = queue.Receive();
                    if (message == null) continue;

                    if (message.Body is MonitoringResultDto monitoringResultDto)
                        moniResults.Add(monitoringResultDto);
                    if (message.Body is BopStateChangedDto bopStateChangedDto)
                        await _msmqMessagesProcessor.ProcessBopStateChanges(bopStateChangedDto);
                }

                portion++;
            }

            if (portion == 0) return 0;
            var result = await _msmqMessagesProcessor.ProcessMultipleMonitoringResults(moniResults);
            return result == 0 ? moniResults.Count : -1;
        }

    }
    public class MsmqHandler : IMsmqHandler
    {
        private readonly IniFile _iniFile;
        private readonly IMyLog _logFile;
        private readonly MsmqMessagesProcessor _msmqMessagesProcessor;


        public MsmqHandler(IniFile iniFile, IMyLog logFile, MsmqMessagesProcessor msmqMessagesProcessor)
        {
            _iniFile = iniFile;
            _logFile = logFile;
            _msmqMessagesProcessor = msmqMessagesProcessor;
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

                await _msmqMessagesProcessor.ProcessMessage(message);
                //                await Task.Factory.StartNew(() => ProcessMessage(message));
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



    }
}
