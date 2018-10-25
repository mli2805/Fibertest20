using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Graph
{
    public static class EventReport
    {
        private static string ForReport(this DateTime timestamp)
        {
            return timestamp.ToString(Thread.CurrentThread.CurrentUICulture.DateTimeFormat.ShortTimePattern) +
                            @" " + timestamp.ToString(Thread.CurrentThread.CurrentUICulture.DateTimeFormat.ShortDatePattern);
        }

        public static string GetShortMessageForNetworkEvent(this Model model, Guid rtuId, bool isMainChannel, bool isOk)
        {
            var rtu = model.Rtus.First(r => r.Id == rtuId);
            var channel = isMainChannel ? Resources.SID_Main_channel : Resources.SID_Reserve_channel;
            var what = isOk ? Resources.SID_Recovered : Resources.SID_Broken;
            return $@"RTU ""{rtu.Title}"" {channel} - {what}";
        }
        public static string GetShortMessageForBopState(this Model model, AddBopNetworkEvent cmd)
        {
            var state = cmd.IsOk ? Resources.SID_Ok : Resources.SID_Breakdown;
            return string.Format(Resources.SID_BOP__0_____1_____2__at__3_, cmd.OtauIp, cmd.TcpPort, state, cmd.EventTimestamp.ForReport());
        }
        public static string GetShortMessageForMonitoringResult(this Model model, MonitoringResultDto dto)
        {
            var trace = model.Traces.FirstOrDefault(t => t.TraceId == dto.PortWithTrace.TraceId);
            if (trace == null) return null;

            return string.Format(Resources.SID_Trace___0___state_is_changed_to___1___at__2_, trace.Title, dto.TraceState, dto.TimeStamp.ForReport());
        }

        public static string GetHtmlReportForMonitoringResult(this Model model, MonitoringResultDto dto)
        {
            var rtu = model.Rtus.FirstOrDefault(r=>r.Id == dto.RtuId);
            if (rtu == null) return null;
            var trace = model.Traces.FirstOrDefault(t => t.TraceId == dto.PortWithTrace.TraceId);
            if (trace == null) return null;

            var content = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + @"Resources\Reports\TraceStateReport.html");
            content = content.Replace(@"@CaptionConst", Resources.SID_Trace_State_Report);
            content = content.Replace(@"@TraceStateConst", Resources.SID_Trace_state);
            content = content.Replace(@"@TraceTitleConst", Resources.SID_Trace);
            content = content.Replace(@"@trace-title", trace.Title);
            content = content.Replace(@"@trace-state", dto.TraceState.ToLocalizedString());
            content = content.Replace(@"@RtuStateConst", @"RTU");
            content = content.Replace(@"@rtu-title", rtu.Title);
            content = content.Replace(@"@PortConst", Resources.SID_Port);
            content = content.Replace(@"@port", dto.PortWithTrace.OtauPort.OpticalPortToString());
            content = content.Replace(@"@DateConst", Resources.SID_Date);
            content = content.Replace(@"@date", dto.TimeStamp.ToString(CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern));
            content = content.Replace(@"@TimeConst", Resources.SID_Time);
            content = content.Replace(@"@time", dto.TimeStamp.ToString(CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern));

            var path = FileOperations.GetParentFolder(AppDomain.CurrentDomain.BaseDirectory) + @"\temp";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            var filename = path + $@"\report-{dto.TimeStamp:yyyy-MM-dd-hh-mm-ss}.html";
            File.WriteAllText(filename, content);
            return filename;
        }
    }
}