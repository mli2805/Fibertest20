using System.Collections.Generic;
using System.Linq;
using Iit.Fibertest.Dto;
using Iit.Fibertest.IitOtdrLibrary;
using Optixsoft.SorExaminer.OtdrDataFormat;
using Optixsoft.SorExaminer.OtdrDataFormat.Structures;

namespace Iit.Fibertest.Graph.Algorithms
{
    public static class SorDataAccidentsExtractorExt
    {
        public static List<AccidentOnTrace> GetAccidents(this OtdrDataKnownBlocks sorData)
        {
            var result = new List<AccidentOnTrace>();

            var levels = sorData.GetRftsEventsBlockForEveryLevel().ToList();

            var level = levels.FirstOrDefault(l => l.LevelName == RftsLevelType.Critical);
            if (level != null && (level.Results & MonitoringResults.IsFailed) != 0)
                foreach (var accidentOnTrace in sorData.GetAccidentsForLevel(level))
                {
                    if (result.All(a => a.RftsEventNumber != accidentOnTrace.RftsEventNumber))
                        result.Add(accidentOnTrace);
                }

            level = levels.FirstOrDefault(l => l.LevelName == RftsLevelType.Major);
            if (level != null && (level.Results & MonitoringResults.IsFailed) != 0)
                foreach (var accidentOnTrace in sorData.GetAccidentsForLevel(level))
                {
                    if (result.All(a => a.RftsEventNumber != accidentOnTrace.RftsEventNumber))
                        result.Add(accidentOnTrace);
                }

            level = levels.FirstOrDefault(l => l.LevelName == RftsLevelType.Minor);
            if (level != null && (level.Results & MonitoringResults.IsFailed) != 0)
                foreach (var accidentOnTrace in sorData.GetAccidentsForLevel(level))
                {
                    if (result.All(a => a.RftsEventNumber != accidentOnTrace.RftsEventNumber))
                        result.Add(accidentOnTrace);
                }

            return result;
        }

        private static IEnumerable<AccidentOnTrace> GetAccidentsForLevel(this OtdrDataKnownBlocks sorData, RftsEventsBlock rftsEventsBlock)
        {
            int newEventsFound = 0;
            for (int i = 1; i < rftsEventsBlock.EventsCount; i++) // 0 - RTU
            {
                var rftsEvent = rftsEventsBlock.Events[i];

                if ((rftsEvent.EventTypes & RftsEventTypes.IsNew) != 0)
                {
                    yield return sorData.BuildAccidentAsNewEvent(rftsEvent, i, rftsEventsBlock.LevelName);
                    newEventsFound++;
                }
                if ((rftsEvent.EventTypes & RftsEventTypes.IsFailed) != 0)
                    yield return sorData.BuildAccidentInOldEvent(rftsEvent, i, newEventsFound, rftsEventsBlock.LevelName);
            }
        }

        private static AccidentOnTrace BuildAccidentInOldEvent(this OtdrDataKnownBlocks sorData, RftsEvent rftsEvent, int i, int newEventsFound, RftsLevelType level)
        {
            var accidentInOldEvent = new AccidentInOldEvent
            {
                RftsEventNumber = i+1, // i - index, i+1 number

                BrokenLandmarkIndex = i - newEventsFound,
                AccidentDistanceKm = sorData.KeyEventDistanceKm(i),
                PreviousLandmarkDistanceKm = sorData.KeyEventDistanceKm(i-1),

                AccidentSeriousness = (rftsEvent.EventTypes & RftsEventTypes.IsFiberBreak) != 0 ? FiberState.FiberBreak : level.ConvertToFiberState(),
                OpticalTypeOfAccident = GetOpticalTypeOfAccident(rftsEvent),
            };


            return accidentInOldEvent;
        }

        private static AccidentOnTrace BuildAccidentAsNewEvent(this OtdrDataKnownBlocks sorData, RftsEvent rftsEvent, int keyEventIndex, RftsLevelType level)
        {
            var baseSorData = sorData.GetBase();

            var leftLandmarkIndex = sorData.GetLandmarkIndexForKeyEventIndex(keyEventIndex-1);
            while (sorData.LinkParameters.LandmarkBlocks[leftLandmarkIndex + 1].Location < sorData.KeyEvents.KeyEvents[keyEventIndex].EventPropagationTime)
            { // to exclude landmarks without events (empty nodes)
                leftLandmarkIndex++;
            }

            // if new event is not a Critical but FiberBreak => there are no events after Break, and in Landmarks there are no event numbers, so take them from Base
            // in base keyEventIndex was the first after break
            var rightLandmarkIndex = baseSorData.GetLandmarkIndexForKeyEventIndex(keyEventIndex);
            while (baseSorData.LinkParameters.LandmarkBlocks[rightLandmarkIndex - 1].Location > sorData.KeyEvents.KeyEvents[keyEventIndex].EventPropagationTime)
            { // to exclude landmarks without events (empty nodes)
                rightLandmarkIndex--;
            }

            var accidentAsNewEvent = new AccidentAsNewEvent()
            {
                RftsEventNumber = keyEventIndex+1, // keyEventIndex - index, keyEventIndex+1 number
                LeftLandmarkIndex = leftLandmarkIndex,
                LeftNodeKm = sorData.LandmarkDistanceKm(leftLandmarkIndex),
                AccidentDistanceKm = sorData.KeyEventDistanceKm(keyEventIndex),
                RightLandmarkIndex = rightLandmarkIndex, // if FiberBreak happens there are no events after Break, so take them from Base
                RightNodeKm = sorData.LandmarkDistanceKm(rightLandmarkIndex),

                AccidentSeriousness = (rftsEvent.EventTypes & RftsEventTypes.IsFiberBreak) != 0 ? FiberState.FiberBreak : level.ConvertToFiberState(),
                OpticalTypeOfAccident = GetOpticalTypeOfAccident(rftsEvent),
            };

            return accidentAsNewEvent;
        }

        private static OpticalAccidentType GetOpticalTypeOfAccident(RftsEvent rftsEvent)
        {
            if ((rftsEvent.EventTypes & RftsEventTypes.IsFiberBreak) != 0)
                return OpticalAccidentType.Break;

            if ((rftsEvent.EventTypes & RftsEventTypes.IsNew) != 0)
                return OpticalAccidentType.Loss;

            if ((rftsEvent.ReflectanceThreshold.Type & ShortDeviationTypes.IsExceeded) != 0)
                return OpticalAccidentType.Reflectance;
            if ((rftsEvent.AttenuationThreshold.Type & ShortDeviationTypes.IsExceeded) != 0)
                return OpticalAccidentType.Loss;
            if ((rftsEvent.AttenuationCoefThreshold.Type & ShortDeviationTypes.IsExceeded) != 0)
                return OpticalAccidentType.LossCoeff;
            return OpticalAccidentType.None;
        }

        private static FiberState ConvertToFiberState(this RftsLevelType level)
        {
            switch (level)
            {
                case RftsLevelType.Minor: return FiberState.Minor;
                case RftsLevelType.Major: return FiberState.Major;
                case RftsLevelType.Critical: return FiberState.Critical;
                default: return FiberState.User;
            }
        }

        private static int GetLandmarkIndexForKeyEventIndex(this OtdrDataKnownBlocks sorData, int keyEventIndex)
        {
            var keyEventNumber = keyEventIndex + 1;
            for (int i = 0; i < sorData.LinkParameters.LandmarksCount; i++)
            {
                if (sorData.LinkParameters.LandmarkBlocks[i].RelatedEventNumber == keyEventNumber)
                    return i;
            }

            return -1;
        }
    }
}