using System;
using System.Collections.Concurrent;
using System.Threading;
using GsmComm.GsmCommunication;
using GsmComm.PduConverter;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.DataCenterCore
{
    public class SmsSender
    {
        private readonly IMyLog _logFile;
        private readonly CurrentDatacenterParameters _currentDatacenterParameters;
        public ConcurrentQueue<SmsSubmitPdu> TheQueue { get; set; } = new ConcurrentQueue<SmsSubmitPdu>();

        public SmsSender(IMyLog logFile, CurrentDatacenterParameters currentDatacenterParameters)
        {
            _logFile = logFile;
            _currentDatacenterParameters = currentDatacenterParameters;
        }

        public void Start()
        {
            var thread = new Thread(SendForever) { IsBackground = true };
            thread.Start();
        }

        // ReSharper disable once FunctionNeverReturns 
        private void SendForever()
        {
            
            // when server just started 
            Thread.Sleep(1000);

            while (true)
            {
                if (TheQueue.TryDequeue(out SmsSubmitPdu sms))
                {
                    // ReSharper disable once UnusedVariable
                    var result = Send(sms);
                }
                else Thread.Sleep(1000);
            }
        }

        private bool Send(SmsSubmitPdu sms)
        {
            var comPort = _currentDatacenterParameters.GsmModemComPort;
            var comm = new GsmCommMain($"COM{comPort}", 9600, 150);
            try
            {
                comm.Open();
            }
            catch (Exception e)
            {
                _logFile.AppendLine("comm.Open: " + e.Message);
                return false;
            }

            var flag = true;
            try
            {
               comm.SendMessage(sms);
            }
            catch (Exception e)
            {
                _logFile.AppendLine("comm.SendMessage: " + e.Message);
                flag = false;
            }

            try
            {
                comm.Close();
            }
            catch (Exception e)
            {
                _logFile.AppendLine("comm.Close: " + e.Message);
                flag = false;
            }

            return flag;
        }

    }
}