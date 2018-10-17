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
        public static string GetShortMessage(this Model model, MonitoringResultDto dto)
        {
            var trace = model.Traces.FirstOrDefault(t => t.TraceId == dto.PortWithTrace.TraceId);
            if (trace == null) return null;

            var dt = dto.TimeStamp.ToString(Thread.CurrentThread.CurrentUICulture.DateTimeFormat.ShortTimePattern) +
                     @" " + dto.TimeStamp.ToString(Thread.CurrentThread.CurrentUICulture.DateTimeFormat.ShortDatePattern);
            switch (dto.TraceState)
            {
                case FiberState.Ok:
                    //return $"Trace {trace.Title} changed it's state to \"OK\" at {dt}";
                    return $"Состояние трассы {trace.Title} стало \"OK\" в {dt}";
                case FiberState.FiberBreak:
                    return string.Format(Resources.SID_Fiber_is_broken_on_trace__0__at__1_, trace.Title, dt);
                case FiberState.NoFiber:
                    return string.Format(Resources.SID_There_is_no_fiber_for_monitoring_on_trace__0__at__1_, trace.Title, dt);
            }

            //var message = $"Trace {trace.Title} state is {dto.TraceState.ToLocalizedString()}";
            var message = $"Состояние трассы {trace.Title} - {dto.TraceState.ToLocalizedString()}";
            return message;
        }

        public static string GetHtmlForMonitoringResult(this Model model, MonitoringResultDto dto)
        {
            var content = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + @"Resources\Reports\TraceStateReport.html");

            var rtu = model.Rtus.FirstOrDefault(r=>r.Id == dto.RtuId);
            if (rtu == null) return null;
            var trace = model.Traces.FirstOrDefault(t => t.TraceId == dto.PortWithTrace.TraceId);
            if (trace == null) return null;
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