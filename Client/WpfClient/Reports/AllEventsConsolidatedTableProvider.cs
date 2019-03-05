using System.Collections.Generic;
using System.Linq;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Client
{
    public static class AllEventsConsolidatedTableProvider
    {
        public static List<List<string>> Create(OpticalEventsDoubleViewModel opticalEventsDoubleViewModel, OpticalEventsReportModel reportModel)
        {
            var result = new List<List<string>>();
            var data = Calculate(opticalEventsDoubleViewModel, reportModel);

            foreach (var eventStatus in EventStatusExt.EventStatusesInRightOrder)
            {
                if (data.ContainsKey(eventStatus))
                    result.Add(Convert(eventStatus, data[eventStatus], reportModel));
            }
            return result;
        }

        private static List<string> Convert(EventStatus eventStatus, 
             Dictionary<FiberState, int> values, OpticalEventsReportModel reportModel)
        {
            var statusLine = new List<string>() { eventStatus.GetLocalizedString() };
            foreach (var state in reportModel.TraceStateSelectionViewModel.GetSelected())
            {
                statusLine.Add(values.ContainsKey(state) ? values[state].ToString() : @"0");
            }
            return statusLine;
        }

        private static Dictionary<EventStatus, Dictionary<FiberState, int>>
            Calculate(OpticalEventsDoubleViewModel opticalEventsDoubleViewModel, OpticalEventsReportModel reportModel)
        {
            var result = new Dictionary<EventStatus, Dictionary<FiberState, int>>();
            foreach (var meas in opticalEventsDoubleViewModel.AllOpticalEventsViewModel.Rows.Where(r => IsEventForReport(r, reportModel)))
            {
                if (result.ContainsKey(meas.EventStatus))
                {
                    if (result[meas.EventStatus].ContainsKey(meas.TraceState))
                    {
                        result[meas.EventStatus][meas.TraceState]++;
                    }
                    else
                    {
                        result[meas.EventStatus].Add(meas.TraceState, 1);
                    }
                }
                else
                {
                    result.Add(meas.EventStatus, new Dictionary<FiberState, int> { { meas.TraceState, 1 } });
                }
            }
            return result;
        }

        private static bool IsEventForReport(this OpticalEventModel opticalEventModel, OpticalEventsReportModel reportModel)
        {
            if (opticalEventModel.MeasurementTimestamp.Date < reportModel.DateFrom.Date) return false;
            if (opticalEventModel.MeasurementTimestamp.Date > reportModel.DateTo.Date) return false;
            if (!reportModel.EventStatusViewModel.GetSelected().Contains(opticalEventModel.EventStatus)) return false;
            return reportModel.TraceStateSelectionViewModel.GetSelected().Contains(opticalEventModel.TraceState);
        }
    }
}