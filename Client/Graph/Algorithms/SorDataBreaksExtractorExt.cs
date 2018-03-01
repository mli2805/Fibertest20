﻿using System.Collections.Generic;
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


            for (int i = 1; i < sorData.RftsEvents.EventsCount; i++) // 0 - RTU
            {
                var rftsEvent = sorData.RftsEvents.Events[i];

                if ((sorData.RftsEvents.Events[i].EventTypes & RftsEventTypes.IsNew) != 0)
                    yield return sorData.BuildAccident(i);
                else
                {
                    if (rftsEvent.ReflectanceThreshold.Deviation < sorData.RftsParameters.Levels[0].LevelThresholdSet
                            .ReflectanceThreshold.RelativeThreshold)
                        continue;

                    
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


            }
        }

        private static AccidentOnTrace BuildAccident(this OtdrDataKnownBlocks sorData, int i)
        {
            var baseSorData = sorData.GetBase();

            var newEvent = new AccidentAsNewEvent()
            {
                LeftNodeKm = sorData.KeyEventDistanceKm(i - 1),
                LeftLandmarkIndex = sorData.GetLandmarkIndexForKeyEvent(i),
                BreakKm = sorData.KeyEventDistanceKm(i),
                RightNodeKm = baseSorData.KeyEventDistanceKm(i), // if FiberBreak happens there are no events after Break, so take them from Base
                RightLandmarkIndex = baseSorData.GetLandmarkIndexForKeyEvent(i + 1),

                AccidentSeriousness = (sorData.RftsEvents.Events[i].EventTypes & RftsEventTypes.IsFiberBreak) == 0 ? FiberState.FiberBreak : FiberState.Critical,
                BreakType = OpticalAccidentType.Break, //TODO
            };

            return newEvent;
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