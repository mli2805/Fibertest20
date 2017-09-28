using System;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using Dto;
using Iit.Fibertest.UtilsLib;

namespace RtuWcfServiceLibrary
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class RtuWcfService : IRtuWcfService
    {
        public static IniFile ServiceIniFile { get; set; }
        public static IMyLog ServiceLog { get; set; }

        public static event OnMessageReceived MessageReceived;
        public delegate void OnMessageReceived(object e);

        private readonly object _lockWcfObj = new object();


        private async Task<RtuInitializedDto> LongTask(InitializeRtuDto dto)
        {
            ServiceLog.AppendLine("Request for long task received...");
            Thread.Sleep(TimeSpan.FromSeconds(10));
            var result = new RtuInitializedDto() { Version = $"I detained {dto.ClientId.First6()} for 10 seconds" };
            return result;
        }

        private async Task<RtuInitializedDto> InitializeAndAnswerAsync(InitializeRtuDto dto)
        {
            
            return await LongTask(dto);
        }

        public RtuInitializedDto InitializeAndAnswer(InitializeRtuDto dto)
        {
            return InitializeAndAnswerAsync(dto).Result;
        }

        public IAsyncResult BeginInitializeAndAnswer(InitializeRtuDto dto, AsyncCallback callback, object retDto)
        {
            var task = InitializeAndAnswerAsync(dto);
            if (callback != null)
                task.ContinueWith(_ => callback(task));
            return task;
        }

        // TODO What this function for?
        public RtuInitializedDto EndInitializeAndAnswer(IAsyncResult result)
        {
            return ((Task<RtuInitializedDto>) result).Result;
        }







        public bool Initialize(InitializeRtuDto dto)
        {
            lock (_lockWcfObj)
            {
                //                ServiceLog.AppendLine("Server sent command: initialize");
                MessageReceived?.Invoke(dto);
                return true;
            }
        }

        public bool StartMonitoring(StartMonitoringDto dto)
        {
            lock (_lockWcfObj)
            {
                //                ServiceLog.AppendLine("Server sent command: start monitoring");
                MessageReceived?.Invoke(dto);
                return true;
            }
        }

        public bool StopMonitoring(StopMonitoringDto dto)
        {
            lock (_lockWcfObj)
            {
                //                ServiceLog.AppendLine("Server sent command: stop monitoring");
                MessageReceived?.Invoke(dto);
                return true;
            }
        }

        public bool ApplyMonitoringSettings(ApplyMonitoringSettingsDto dto)
        {
            lock (_lockWcfObj)
            {
                //                ServiceLog.AppendLine("Server sent command: apply monitoring settings");
                MessageReceived?.Invoke(dto);
                return true;
            }
        }

        public bool AssignBaseRef(AssignBaseRefDto dto)
        {
            lock (_lockWcfObj)
            {
                //                ServiceLog.AppendLine("Server sent command: assign base ref");
                MessageReceived?.Invoke(dto);
                return true;
            }
        }

        public bool ToggleToPort(OtauPortDto dto)
        {
            lock (_lockWcfObj)
            {
                //                ServiceLog.AppendLine("Server sent command: toggle to port");
                MessageReceived?.Invoke(dto);
                return true;
            }
        }

        public bool CheckLastSuccessfullMeasTime()
        {
            lock (_lockWcfObj)
            {
                ServiceLog.AppendLine("WatchDog asks time of last successfull measurement");
                MessageReceived?.Invoke(new LastSuccessfullMeasTimeDto());
                return true;
            }
        }
    }
}