using System;
using System.ServiceModel;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.RtuWcfServiceInterface;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.RtuManagement
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class RtuWcfService : IRtuWcfService
    {
        private IniFile _serviceIniFile;
        private readonly IMyLog _serviceLog;
        private readonly RtuManager _rtuManager;

        public static event OnMessageReceived MessageReceived;
        public delegate void OnMessageReceived(object e);

        private readonly object _lockWcfObj = new object();


        public RtuWcfService(IniFile serviceIniFile, IMyLog serviceLog, RtuManager rtuManager)
        {
            _serviceIniFile = serviceIniFile;
            _serviceLog = serviceLog;
            _rtuManager = rtuManager;
        }


        private async Task<RtuInitializedDto> InitializeRtuAsync(InitializeRtuDto dto)
        {
            _serviceLog.AppendLine("Request for long task received...");
            await TaskEx.Delay(TimeSpan.FromSeconds(3));
            _serviceLog.AppendLine("Request for long task 2");
            return new RtuInitializedDto
            {
                Version = $"I detained {dto.ClientId.First6()} for 3 seconds"
            };
        }


        public IAsyncResult BeginInitializeRtu(InitializeRtuDto dto, AsyncCallback callback, object asyncState)
        {
            _serviceLog.AppendLine("point 11");

            var task = InitializeRtuAsync(dto);
            if (callback != null)
                task.ContinueWith(_ => callback(task));
            return task;

            //            return InitializeRtuAsync(dto);
        }

        public RtuInitializedDto EndInitializeRtu(IAsyncResult result)
        {
            _serviceLog.AppendLine("point 21");
            return ((Task<RtuInitializedDto>)result).Result;
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
                _serviceLog.AppendLine("Server sent command: stop monitoring");
            _rtuManager.StopMonitoring();
            return true;

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
                _serviceLog.AppendLine("WatchDog asks time of last successfull measurement");
                MessageReceived?.Invoke(new LastSuccessfullMeasTimeDto());
                return true;
            }
        }
    }
}