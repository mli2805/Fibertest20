using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Graph
{
    public static class EventReport
    {
        public static string GetShortMessage(this Model model, MonitoringResultDto dto)
        {
            var trace = model.Traces.FirstOrDefault(t => t.TraceId == dto.PortWithTrace.TraceId);
            if (trace == null) return null;

            switch (dto.TraceState)
            {
                case FiberState.Ok:
                    return $"Trace {trace.Title} change it's state to \"OK\" at {dto.TimeStamp}";
                case FiberState.FiberBreak:
                    return $"Fiber is broken on trace {trace.Title} at {dto.TimeStamp}";
                case FiberState.NoFiber:
                    return $"There is no fiber for monitoring on trace {trace.Title} at {dto.TimeStamp}";
            }

            var message = $"Trace {trace.Title} state is {dto.TraceState.ToLocalizedString()}";
            return message;
        }

        public static string GetHtmlForMonitoringResult(this Model model, MonitoringResultDto dto)
        {
            var content = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + @"Resources\Reports\TraceStateReport.html");

            var trace = model.Traces.FirstOrDefault(t => t.TraceId == dto.PortWithTrace.TraceId);
            if (trace == null) return null;
            content = content.Replace(@"#LinkState", Resources.SID_Trace_state);
            content = content.Replace(@"@trace-title", trace.Title);

            var filename = @"c:\temp.html";
            File.WriteAllText(filename, content);
            return filename;
        }
    }
}