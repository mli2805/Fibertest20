using System;
using System.Collections.Generic;
using System.Linq;
using Iit.Fibertest.Dto;
using Iit.Fibertest.IitOtdrLibrary;
using Iit.Fibertest.UtilsLib;
using Optixsoft.SorExaminer.OtdrDataFormat;
using Optixsoft.SorExaminer.OtdrDataFormat.Structures;

namespace Iit.Fibertest.Graph
{
   
    public class AccidentsFromSorExtractor
    {
        private readonly IMyLog _logFile;
        private readonly SorDataParsingReporter _sorDataParsingReporter;
        private readonly Model _writeModel;
        private OtdrDataKnownBlocks _sorData;

        public AccidentsFromSorExtractor(IMyLog logFile, SorDataParsingReporter sorDataParsingReporter, Model writeModel)
        {
            _logFile = logFile;
            _sorDataParsingReporter = sorDataParsingReporter;
            _writeModel = writeModel;
        }

        public List<AccidentOnTrace> GetAccidents(OtdrDataKnownBlocks sorData, bool isForDebug)
        {
            _sorData = sorData;

            try
            {
                return GetAccidents(isForDebug);
            }
            catch (Exception e)
            {
                _logFile.AppendLine(@"GetAccidents: " + e.Message);
                return new List<AccidentOnTrace>();
            }
        }

        private List<AccidentOnTrace> GetAccidents(bool isForDebug)
        {
            if (isForDebug)
                _sorDataParsingReporter.DoReport(_sorData);

            var levels = new List<RftsLevelType>() {RftsLevelType.Critical, RftsLevelType.Major, RftsLevelType.Minor};
            var result = new List<AccidentOnTrace>();
            var rftsEventsBlocks = _sorData.GetRftsEventsBlockForEveryLevel().ToList();

            foreach (var level in levels)
            {
                var rftsBlockForLevel = rftsEventsBlocks.FirstOrDefault(l => l.LevelName == level);
                if (rftsBlockForLevel != null && (rftsBlockForLevel.Results & MonitoringResults.IsFailed) != 0)
                {
                    foreach (var accidentOnTrace in GetAccidentsForLevel(rftsBlockForLevel))
                    {
                        if (!IsDuplicate(result, accidentOnTrace))
                            result.Add(accidentOnTrace);
                    }
                }
            }

            return result;
        }

        private bool IsDuplicate(List<AccidentOnTrace> alreadyFound, AccidentOnTrace accident)
        {
            if (alreadyFound.Any(a => a.BrokenRftsEventNumber == accident.BrokenRftsEventNumber &&
                                a.OpticalTypeOfAccident == OpticalAccidentType.Break))
                return true;

            if (accident.OpticalTypeOfAccident == OpticalAccidentType.LossCoeff &&
                alreadyFound.Any(a => a.BrokenRftsEventNumber == accident.BrokenRftsEventNumber &&
                                a.OpticalTypeOfAccident == OpticalAccidentType.LossCoeff))
                return true;

            if ((accident.OpticalTypeOfAccident == OpticalAccidentType.Reflectance || accident.OpticalTypeOfAccident == OpticalAccidentType.Loss) &&
                alreadyFound.Any(a => a.BrokenRftsEventNumber == accident.BrokenRftsEventNumber &&
                                (a.OpticalTypeOfAccident == OpticalAccidentType.Reflectance || a.OpticalTypeOfAccident == OpticalAccidentType.Loss)))
                return true;

            return false;
        }

        private IEnumerable<AccidentOnTrace> GetAccidentsForLevel(RftsEventsBlock rftsEventsBlock)
        {
            for (int keyEventIndex = 1; keyEventIndex < rftsEventsBlock.EventsCount; keyEventIndex++) // 0 - RTU
            {
                var rftsEvent = rftsEventsBlock.Events[keyEventIndex];

                if ((rftsEvent.EventTypes & RftsEventTypes.IsNew) != 0)
                    yield return BuildAccidentAsNewEvent(rftsEvent, keyEventIndex, rftsEventsBlock.LevelName);
                if ((rftsEvent.EventTypes & RftsEventTypes.IsFailed) != 0 || (rftsEvent.EventTypes & RftsEventTypes.IsFiberBreak) != 0)
                    foreach (var opticalAccidentType in GetOpticalTypesOfAccident(rftsEvent))
                    {
                        var accident = BuildAccidentInOldEvent(rftsEvent, keyEventIndex, rftsEventsBlock.LevelName);
                        accident.OpticalTypeOfAccident = opticalAccidentType;
                        yield return accident;
                    }
            }
        }
      
        private AccidentOnTrace BuildAccidentInOldEvent(RftsEvent rftsEvent, int keyEventIndex, RftsLevelType level)
        {
            var brokenLandmarkIndex = _sorData.GetLandmarkIndexForKeyEventIndex(keyEventIndex);
            if (brokenLandmarkIndex == -1)
            {
                // event was not bound to landmark and now it gets worse
                return BuildAccidentAsNewEvent(rftsEvent, keyEventIndex, level);
            }
            var previousLandmark = _sorData.LinkParameters.LandmarkBlocks[brokenLandmarkIndex - 1];

            var accidentInOldEvent = new AccidentInOldEvent
            {
                BrokenRftsEventNumber = keyEventIndex + 1, // i - index, i+1 number

                BrokenLandmarkIndex = brokenLandmarkIndex,
                AccidentDistanceKm = _sorData.KeyEventDistanceKm(keyEventIndex),
                PreviousLandmarkDistanceKm = _sorData.OwtToLenKm(previousLandmark.Location),

                AccidentSeriousness = (rftsEvent.EventTypes & RftsEventTypes.IsFiberBreak) != 0 ? FiberState.FiberBreak : level.ConvertToFiberState(),
                OpticalTypeOfAccident = GetOpticalTypeOfAccident(rftsEvent),
            };

            return accidentInOldEvent;
        }

        private AccidentOnTrace BuildAccidentAsNewEvent(RftsEvent rftsEvent, int keyEventIndex, RftsLevelType level)
        {
            var leftLandmarkIndex = _sorData.GetLandmarkToTheLeftFromOwt(_sorData.KeyEvents.KeyEvents[keyEventIndex].EventPropagationTime);
            var rightLandmarkIndex = leftLandmarkIndex + 1;

            var accidentAsNewEvent = new AccidentAsNewEvent()
            {
                BrokenRftsEventNumber = keyEventIndex + 1, // keyEventIndex - index, keyEventIndex+1 number
                LeftLandmarkIndex = leftLandmarkIndex,
                LeftNodeKm = _sorData.LandmarkDistanceKm(leftLandmarkIndex),
                AccidentDistanceKm = _sorData.KeyEventDistanceKm(keyEventIndex),
                RightLandmarkIndex = rightLandmarkIndex, // if FiberBreak happens there are no events after Break, so take them from Base
                RightNodeKm = _sorData.LandmarkDistanceKm(rightLandmarkIndex),

                AccidentSeriousness = (rftsEvent.EventTypes & RftsEventTypes.IsFiberBreak) != 0 ? FiberState.FiberBreak : level.ConvertToFiberState(),
                OpticalTypeOfAccident = GetOpticalTypeOfAccident(rftsEvent),
            };

            return accidentAsNewEvent;
        }

        private OpticalAccidentType GetOpticalTypeOfAccident(RftsEvent rftsEvent)
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

        private IEnumerable<OpticalAccidentType> GetOpticalTypesOfAccident(RftsEvent rftsEvent)
        {
            if ((rftsEvent.EventTypes & RftsEventTypes.IsFiberBreak) != 0)
                yield return OpticalAccidentType.Break;
            else
            {
                if ((rftsEvent.ReflectanceThreshold.Type & ShortDeviationTypes.IsExceeded) != 0)
                    yield return OpticalAccidentType.Reflectance;
                if ((rftsEvent.AttenuationThreshold.Type & ShortDeviationTypes.IsExceeded) != 0)
                    yield return OpticalAccidentType.Loss;
                if ((rftsEvent.AttenuationCoefThreshold.Type & ShortDeviationTypes.IsExceeded) != 0)
                    yield return OpticalAccidentType.LossCoeff;
            }
        }
    }
}