using System.Collections.Generic;
using System.Linq;
using Iit.Fibertest.Dto;
using Iit.Fibertest.IitOtdrLibrary;
using Optixsoft.SorExaminer.OtdrDataFormat;

namespace Iit.Fibertest.Graph.Algorithms
{
    public static class SorDataBreaksExtractorExt
    {
        public static IEnumerable<AccidentOnTrace> GetAccidents(this OtdrDataKnownBlocks sorData)
        {
            for (int i = 0; i < sorData.RftsEvents.EventsCount; i++)
            {
                var rftsEvent = sorData.RftsEvents.Events[i];
                if (rftsEvent.EventTypes == RftsEventTypes.IsFailed ||
                    rftsEvent.EventTypes == RftsEventTypes.IsFiberBreak)
                {
                    var brokenLandmark = sorData.LinkParameters.LandmarkBlocks.FirstOrDefault(l => l.RelatedEventNumber == i + 1);
                    if (brokenLandmark == null) continue;

                    var accident = new AccidentInOldEvent()
                    {
                        EventNumber = brokenLandmark.Number,
                        BreakKm = brokenLandmark.Location,
                        AccidentSeriousness = rftsEvent.EventTypes == RftsEventTypes.IsFiberBreak ? FiberState.FiberBreak : FiberState.Critical, //TODO
                        BreakType = OpticalAccidentType.Reflectance, // TODO
                    };

                    yield return accident;
                }

                if ((sorData.RftsEvents.Events[i].EventTypes & RftsEventTypes.IsNew) != 0)
                {
                    var baseSorData = sorData.GetBase();

                    var newEvent = new AccidentAsNewEvent()
                    {
                        LeftNodeKm = sorData.KeyEventDistanceKm(i-1),
                        LeftLandmarkIndex = sorData.GetLandmarkIndexForKeyEvent(i),
                        BreakKm = sorData.KeyEventDistanceKm(i),
                        RightNodeKm = baseSorData.KeyEventDistanceKm(i), // if FiberBreak happens there are no events after Break, so take them from Base
                        RightLandmarkIndex = baseSorData.GetLandmarkIndexForKeyEvent(i + 1),

                        AccidentSeriousness = (sorData.RftsEvents.Events[i].EventTypes & RftsEventTypes.IsFiberBreak) == 0 ? FiberState.FiberBreak : FiberState.Critical,
                        BreakType = OpticalAccidentType.Break, //TODO
                    };
                     
                    yield return newEvent;
                }
            }
        }

        private static int GetLandmarkIndexForKeyEvent(this OtdrDataKnownBlocks sorData, int keyEventNumber)
        {
            for (int i = 0; i < sorData.LinkParameters.LandmarksCount; i++)
            {
                if (sorData.LinkParameters.LandmarkBlocks[i].RelatedEventNumber == keyEventNumber)
                    return i;
            }

            return -1;
        }
    }
}