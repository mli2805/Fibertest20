using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Newtonsoft.Json;
using System.IO;
using System.Messaging;
using System;

namespace Iit.Fibertest.RtuManagement
{
    public partial class RtuManager
    {
        private void SendByMsmqIfAnyOnDisk()
        {
            var baseFolder = FileOperations.GetParentFolder(AppDomain.CurrentDomain.BaseDirectory);
            var msmqFileName = Path.Combine(baseFolder, @"ini\msmq.json");

            if (File.Exists(msmqFileName))
            {
                _rtuLog.AppendLine("msmq.json found!");
                var json = File.ReadAllText(msmqFileName);
                var dto = JsonConvert.DeserializeObject(json, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All });
                SendByMsmq(dto);
            }
        }

        private void SendByMsmq(object dto)
        {
            if (!TrySendByMsmq(dto))
            {
                return ;
            }

            switch (dto)
            {
                case MonitoringResultDto _:
                    _rtuLog.AppendLine("Monitoring result sent by MSMQ."); break;
                case BopStateChangedDto _:
                    _rtuLog.AppendLine("OTAU state changes sent by MSMQ"); break;
            }

        }

        /// <summary>
        /// stops RTU service if failed
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private bool TrySendByMsmq(object dto)
        {
            var baseFolder = FileOperations.GetParentFolder(AppDomain.CurrentDomain.BaseDirectory);
            var msmqFileName = Path.Combine(baseFolder, @"ini\msmq.json");

            try
            {
                var json = JsonConvert.SerializeObject(dto, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All });
                File.WriteAllText(msmqFileName, json);
            }
            catch (Exception e)
            {
                _rtuLog.AppendLine("Write MSMQ file: " + e.Message);
                return false;
            }

            try
            {
                Message message = new Message(dto, new BinaryMessageFormatter());
                var address = _serviceIni.Read(IniSection.ServerMainAddress, IniKey.Ip, "0.0.0.0");
                var connectionString = $@"FormatName:DIRECT=TCP:{address}\private$\Fibertest20";
                var queue = new MessageQueue(connectionString);
                queue.Send(message, MessageQueueTransactionType.Single);
            }
            catch (Exception e)
            {
                _rtuLog.AppendLine("TrySendByMsmq: " + e.Message);

                _serviceIni.Write(IniSection.Recovering, IniKey.RecoveryStep, (int)RecoveryStep.RestartService);
                _rtuLog.AppendLine("Restart RTU service.");
                _serviceLog.AppendLine("Restart RTU service.");
                Environment.Exit(1);
                return false;
            }

            try
            {
                File.Delete(msmqFileName);
            }
            catch (Exception e)
            {
                _rtuLog.AppendLine("Delete MSMQ file: " + e.Message);
                return true; // msmq message was sent successfully
            }

            return true;
        }
    }
}
