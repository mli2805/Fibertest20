using System;
using System.Collections.Concurrent;
using System.Threading;
using GsmComm.GsmCommunication;
using GsmComm.PduConverter;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.DataCenterCore
{
    public class SmsSender
    {
        private readonly IniFile _iniFile;
        private readonly IMyLog _logFile;
        public ConcurrentQueue<SmsSubmitPdu> TheQueue { get; set; } = new ConcurrentQueue<SmsSubmitPdu>();

        public SmsSender(IniFile iniFile, IMyLog logFile)
        {
            _iniFile = iniFile;
            _logFile = logFile;
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
                    Send(sms);
                }
                else Thread.Sleep(1000);
            }
        }

        private bool Send(SmsSubmitPdu sms)
        {
            var comPort = _iniFile.Read(IniSection.Broadcast, IniKey.GsmModemComPort, 0);
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