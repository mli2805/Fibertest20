using System;
using System.ServiceModel;
using System.Threading;
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


        public void BeginInitialize(InitializeRtuDto dto)
        {
            _serviceLog.AppendLine("User demands initialization - OK");
            var callbackChannel = OperationContext.Current.GetCallbackChannel<IRtuWcfServiceBackward>();
            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    _rtuManager.Initialize(dto);
                    _serviceLog.AppendLine("Initialization terminated");
                    callbackChannel.EndInitialize(_rtuManager.GetInitializationResult());
                }
                catch (Exception e)
                {
                    _serviceLog.AppendLine("Thread pool: " + e);
                }
            });
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