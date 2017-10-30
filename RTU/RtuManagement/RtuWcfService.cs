﻿using System;
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
                    _rtuManager.Initialize(dto, () => callbackChannel.EndInitialize(_rtuManager.GetInitializationResult()));
                }
                catch (Exception e)
                {
                    _serviceLog.AppendLine("Thread pool: " + e);
                }
            });
        }


        public void BeginStartMonitoring(StartMonitoringDto dto)
        {
            var callbackChannel = OperationContext.Current.GetCallbackChannel<IRtuWcfServiceBackward>();
            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    if (ShouldStart())
                        _rtuManager.StartMonitoring(() => callbackChannel.EndStartMonitoring(_rtuManager.IsMonitoringOn));
                    else
                        callbackChannel.EndStartMonitoring(_rtuManager.IsMonitoringOn);
                }
                catch (Exception e)
                {
                    _serviceLog.AppendLine("Thread pool: " + e);
                }
            });
        }

        private bool ShouldStart()
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
            _serviceLog.AppendLine("User demands start monitoring");
            return true;
        }

        public void BeginStopMonitoring(StopMonitoringDto dto)
        {
            var callbackChannel = OperationContext.Current.GetCallbackChannel<IRtuWcfServiceBackward>();
            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    if (ShouldStop())
                        _rtuManager.StopMonitoring();
                }
                catch (Exception e)
                {
                    _serviceLog.AppendLine("Thread pool: " + e);
                }
                _serviceLog.AppendLine($"StopMonitoring terminated {_rtuManager.IsMonitoringOn}");
                callbackChannel.EndStopMonitoring(_rtuManager.IsMonitoringOn);
            });

        }

        private bool ShouldStop()
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
            _serviceLog.AppendLine("User demands stop monitoring");
            return true;
        }


        public void BeginApplyMonitoringSettings(ApplyMonitoringSettingsDto dto)
        {
            var callbackChannel = OperationContext.Current.GetCallbackChannel<IRtuWcfServiceBackward>();
            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    if (ShouldApplySettings())
                        _rtuManager.ChangeSettings(dto);
                    _serviceLog.AppendLine("Monitoring settings applied");
                }
                catch (Exception e)
                {
                    _serviceLog.AppendLine("Thread pool: " + e);
                }
                callbackChannel.EndApplyMonitoringSettings(_rtuManager.IsMonitoringOn);
            });
        }

        private bool ShouldApplySettings()
        {
            if (!_rtuManager.IsRtuInitialized)
            {
                _serviceLog.AppendLine("User sent monitoring settings - Ignored - RTU is busy");
                return false;
            }
            _serviceLog.AppendLine("User sent monitoring settings - Accepted");
            return true;
        }

        public void BeginAssignBaseRef(AssignBaseRefDto dto)
        {
            var callbackChannel = OperationContext.Current.GetCallbackChannel<IRtuWcfServiceBackward>();
            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    if (ShouldAssignBaseRef())
                        _rtuManager.AssignBaseRefs(dto);
                    _serviceLog.AppendLine("Base ref assigned");
                }
                catch (Exception e)
                {
                    _serviceLog.AppendLine("Thread pool: " + e);
                }
                callbackChannel.EndAssignBaseRef(_rtuManager.IsMonitoringOn);
            });
        }
        private bool ShouldAssignBaseRef()
        {
            if (!_rtuManager.IsRtuInitialized)
            {
                _serviceLog.AppendLine("User sent base ref - Ignored - RTU is busy");
                return false;
            }
            _serviceLog.AppendLine("User sent base ref - Accepted");
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