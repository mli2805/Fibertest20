using System.Collections.Generic;
using System.Linq;
using Iit.Fibertest.Dto;
using Iit.Fibertest.IitOtdrLibrary;
using Optixsoft.SorExaminer.OtdrDataFormat;
using Optixsoft.SorExaminer.OtdrDataFormat.Structures;

namespace Iit.Fibertest.Graph.Algorithms
{
    public static class SorDataBreaksExtractorExt
    {
        public static List<AccidentOnTrace> GetAccidents(this OtdrDataKnownBlocks sorData)
        {
            var result = new List<AccidentOnTrace>();

            var levels = sorData.GetRftsEventsBlockForEveryLevel().ToList();

            var level = levels.FirstOrDefault(l => l.LevelName == RftsLevelType.Critical);
            if (level != null && (level.Results & MonitoringResults.IsFailed) != 0)
                foreach (var accidentOnTrace in sorData.GetAccidentsForLevel(level))
                {
                    if (result.All(a => a.KeyEventNumber != accidentOnTrace.KeyEventNumber))
                        result.Add(accidentOnTrace);
                }

            level = levels.FirstOrDefault(l => l.LevelName == RftsLevelType.Major);
            if (level != null && (level.Results & MonitoringResults.IsFailed) != 0)
                foreach (var accidentOnTrace in sorData.GetAccidentsForLevel(level))
                {
                    if (result.All(a => a.KeyEventNumber != accidentOnTrace.KeyEventNumber))
                        result.Add(accidentOnTrace);
                }

            level = levels.FirstOrDefault(l => l.LevelName == RftsLevelType.Minor);
            if (level != null && (level.Results & MonitoringResults.IsFailed) != 0)
                foreach (var accidentOnTrace in sorData.GetAccidentsForLevel(level))
                {
                    if (result.All(a => a.KeyEventNumber != accidentOnTrace.KeyEventNumber))
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
                BrokenLandmarkIndex = i - newEventsFound,
                AccidentDistanceKm = sorData.KeyEventDistanceKm(i),
                PreviousLandmarkDistanceKm = sorData.KeyEventDistanceKm(i-1),

                AccidentSeriousness = (rftsEvent.EventTypes & RftsEventTypes.IsFiberBreak) != 0 ? FiberState.FiberBreak : level.ConvertToFiberState(),
                OpticalTypeOfAccident = GetOpticalTypeOfAccident(rftsEvent),
            };


            return accidentInOldEvent;
        }
        private static AccidentOnTrace BuildAccidentAsNewEvent(this OtdrDataKnownBlocks sorData, RftsEvent rftsEvent, int i, RftsLevelType level)
        {
            var baseSorData = sorData.GetBase();

            var accidentAsNewEvent = new AccidentAsNewEvent()
            {
                KeyEventNumber = i+1,
                LeftNodeKm = sorData.KeyEventDistanceKm(i - 1),
                LeftLandmarkIndex = sorData.GetLandmarkIndexForKeyEvent(i),
                AccidentDistanceKm = sorData.KeyEventDistanceKm(i),
                RightNodeKm = baseSorData.KeyEventDistanceKm(i), // if FiberBreak happens there are no events after Break, so take them from Base
                RightLandmarkIndex = baseSorData.GetLandmarkIndexForKeyEvent(i + 1),

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