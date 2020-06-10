using System;
using System.Messaging;
using System.Threading;
using System.Threading.Tasks;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.DataCenterCore
{
    public interface IMsmqHandler
    {
        void Start();

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
                await _msmqMessagesProcessor.ProcessMessage(message); // synchronous operation
            }
            catch (Exception e)
            {
                _logFile.AppendLine($"MyReceiveCompleted: {e.Message}");
                await Task.Delay(1000);
            }
            finally
            {
                // Restart the asynchronous receive operation.
                queue.BeginReceive();
            }
        }



    }
}
