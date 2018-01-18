using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.RtuManagement
{
    public class RtuWcfOperationsPermissions
    {
        private readonly IMyLog _serviceLog;
        private readonly RtuManager _rtuManager;

        public RtuWcfOperationsPermissions(IMyLog serviceLog, RtuManager rtuManager)
        {
            _serviceLog = serviceLog;
            _rtuManager = rtuManager;
        }

        public bool ShouldStop()
        {
            if (!_rtuManager.IsRtuInitialized)
            {
                _serviceLog.AppendLine("User stops monitoring - Ignored - RTU is busy");
                return false;
            }
            if (!_rtuManager.IsMonitoringOn)
            {
                _serviceLog.AppendLine("User stops monitoring - Ignored - MANUAL mode already");
                return false;
            }
            _serviceLog.AppendLine("User demands stop monitoring");
            return true;
        }

        public bool ShouldPerformOtauOperation()
        {
            if (!_rtuManager.IsRtuInitialized)
            {
                _serviceLog.AppendLine("User asks OTAU operation - Ignored - RTU is busy");
                return false;
            }
            if (_rtuManager.IsMonitoringOn)
            {
                _serviceLog.AppendLine("User asks OTAU operation - Ignored - RTU in AUTOMATIC mode");
                return false;
            }
            _serviceLog.AppendLine("User demands OTAU operation");
            return true;
        }

        // Apply monitoring settings and Assign base refs
        public bool ShouldExecute(string message)
        {
            if (!_rtuManager.IsRtuInitialized)
            {
                _serviceLog.AppendLine($"{message} - Ignored - RTU is busy");
                return false;
            }
            _serviceLog.AppendLine($"{message} - Accepted");
            return true;
        }

    }
}