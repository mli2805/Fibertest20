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
        private readonly IMyLog _serviceLog;
        private readonly RtuManager _rtuManager;

        public RtuWcfService(IMyLog serviceLog, RtuManager rtuManager)
        {
            _serviceLog = serviceLog;
            _rtuManager = rtuManager;
        }


        private async Task<RtuInitializedDto> InitializeRtuAsync(InitializeRtuDto dto)
        {
            _serviceLog.AppendLine("Request for long task received...");
            await TaskEx.Delay(TimeSpan.FromSeconds(3));
//                        _rtuManager.Initialize(dto);
            _serviceLog.AppendLine("Request for long task 2");
            return new RtuInitializedDto
            {
                Version = $"I detained {dto.ClientId.First6()} for 3 seconds"
            };
        }


        public IAsyncResult BeginInitializeRtu(InitializeRtuDto dto, AsyncCallback callback, object asyncState)
        {
            _serviceLog.AppendLine("User demands async initialization - OK");

//            var task = InitializeRtuAsync(dto);
//            if (callback != null)
//                task.ContinueWith(_ => callback(task));
//            return task;

                        return InitializeRtuAsync(dto);
        }

        public RtuInitializedDto EndInitializeRtu(IAsyncResult result)
        {
            _serviceLog.AppendLine("point 21");
            return ((Task<RtuInitializedDto>)result).Result;
        }







        public bool Initialize(InitializeRtuDto dto)
        {
            _serviceLog.AppendLine("User demands initialization - OK");
            _rtuManager.Initialize(dto);
            return true;
        }

        public bool StartMonitoring(StartMonitoringDto dto)
        {
            if (!_rtuManager.IsRtuInitialized)
            {
                _serviceLog.AppendLine("User starts monitoring - Ignored - RTU is busy");
                return false;
            }
            if (_rtuManager.IsMonitoringOn)
            {
                _serviceLog.AppendLine("User starts monitoring - Ignored - AUTOMATIC mode already");
                return false;
            }

            _serviceLog.AppendLine("User starts monitoring");
            _rtuManager.StartMonitoring();
            return true;
        }

        public bool StopMonitoring(StopMonitoringDto dto)
        {
            if (!_rtuManager.IsRtuInitialized)
            {
                _serviceLog.AppendLine("User stops monitoring - Ignored - RTU is busy");
                return false;
            }
            if (!_rtuManager.IsMonitoringOn)
            {
                _serviceLog.AppendLine("User starts monitoring - Ignored - MANUAL mode already");
                return false;
            }

            _serviceLog.AppendLine("User stops monitoring");
            _rtuManager.StopMonitoring();
            return true;

        }

        public bool ApplyMonitoringSettings(ApplyMonitoringSettingsDto dto)
        {
            if (_rtuManager.IsRtuInitialized)
            {
                _rtuManager.ChangeSettings(dto);
                _serviceLog.AppendLine("User sent monitoring settings - Accepted");
            }
            else
                _serviceLog.AppendLine("User sent monitoring settings - Ignored - RTU is busy");
            return true;
        }

        public bool AssignBaseRef(AssignBaseRefDto dto)
        {
            if (!_rtuManager.IsMonitoringOn)
            {
                _serviceLog.AppendLine("User sent base ref - Accepted");
                _rtuManager.AssignBaseRefs(dto);
            }
            else
            {
                _serviceLog.AppendLine("User sent base ref - Ignored - RTU is busy");
            }
            return true;
        }

        public bool ToggleToPort(OtauPortDto dto)
        {
            if (!_rtuManager.IsRtuInitialized || _rtuManager.IsMonitoringOn)
            {
                _serviceLog.AppendLine("User demands port toggle - Ignored - RTU is busy");
                return false;
            }

            _serviceLog.AppendLine("User demands port toggle");
            _rtuManager.ToggleToPort(dto);
            return true;
        }

        public bool CheckLastSuccessfullMeasTime()
        {

            _serviceLog.AppendLine("WatchDog asks time of last successfull measurement");
            //            _rtuManager.
            return true;
        }
    }
}