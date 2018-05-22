using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public static class LogOperationCodeExt
    {
        public static string GetLocalizedString(this LogOperationCode code)
        {
            switch (code)
            {
                case LogOperationCode.ClientStarted: return Resources.SID_Client_started;
                case LogOperationCode.ClientExited: return Resources.SID_Client_exited;

                case LogOperationCode.RtuAdded: return Resources.SID_RTU_added;
                case LogOperationCode.RtuUpdated: return Resources.SID_RTU_updated;
                case LogOperationCode.RtuInitialized: return Resources.SID_RTU_initialized2;

                case LogOperationCode.TraceAdded: return Resources.SID_Trace_added;
                case LogOperationCode.TraceUpdated: return Resources.SID_Trace_updated;
                case LogOperationCode.TraceAttached: return Resources.SID_Trace_attached;
                case LogOperationCode.TraceDetached: return Resources.SID_Trace_detached;
                case LogOperationCode.TraceCleaned: return Resources.SID_Trace_cleaned;
                case LogOperationCode.TraceRemoved: return Resources.SID_Trace_removed;

                case LogOperationCode.BaseRefAssigned: return Resources.SID_Base_ref_assigned;
                case LogOperationCode.MonitoringSettingsChanged: return Resources.SID_Monitoring_settings_changed;
                case LogOperationCode.MonitoringStopped: return Resources.SID_Monitoring_stopped;

                default: return Resources.SID_Unknown_code;
            }
        }
    }
}