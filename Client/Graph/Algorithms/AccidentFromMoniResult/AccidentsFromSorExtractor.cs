using System;
using System.Collections.Generic;
using System.Linq;
using Iit.Fibertest.Dto;
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
        private readonly AccidentPlaceLocator _accidentPlaceLocator;
        private OtdrDataKnownBlocks _sorData;

        private Guid _traceId;
        private List<Node> _nodesExcludingAdjustmentPoints;
        private Rtu _rtu;
        private List<Equipment> _equipmentsWithoutPointsAndRtu;

        public AccidentsFromSorExtractor(IMyLog logFile, Model writeModel,
            AccidentPlaceLocator accidentPlaceLocator, SorDataParsingReporter sorDataParsingReporter)
        {
            _logFile = logFile;
            _sorDataParsingReporter = sorDataParsingReporter;
            _writeModel = writeModel;
            _accidentPlaceLocator = accidentPlaceLocator;
        }

        public List<AccidentOnTraceV2> GetAccidents(OtdrDataKnownBlocks sorData, Guid traceId, bool isForDebug)
        {
            _sorData = sorData;
            _traceId = traceId;
            try
            {
                _writeModel.GetTraceForAccidentDefine(traceId,
                    out _rtu, out _nodesExcludingAdjustmentPoints, out _equipmentsWithoutPointsAndRtu);
                return GetAccidents(isForDebug);
            }
            catch (Exception e)
            {
                _logFile.AppendLine(@"GetAccidents: " + e.Message);
                return new List<AccidentOnTraceV2>();
            }
        }

        private List<AccidentOnTraceV2> GetAccidents(bool isForDebug)
        {
            if (isForDebug)
                _sorDataParsingReporter.DoReport(_sorData);

            var levels = new List<RftsLevelType>() { RftsLevelType.Critical, RftsLevelType.Major, RftsLevelType.Minor };
            var result = new List<AccidentOnTraceV2>();
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

        private bool IsDuplicate(List<AccidentOnTraceV2> alreadyFound, AccidentOnTraceV2 accident)
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

            if (accident.OpticalTypeOfAccident == OpticalAccidentType.TotalLoss &&
                alreadyFound.Any(a=> a.OpticalTypeOfAccident == OpticalAccidentType.TotalLoss))
                return true;

            return false;
        }

        private IEnumerable<AccidentOnTraceV2> GetAccidentsForLevel(RftsEventsBlock rftsEventsBlock)
        {
            for (int keyEventIndex = 1; keyEventIndex < rftsEventsBlock.EventsCount; keyEventIndex++) // 0 - RTU
            {
                var rftsEvent = rftsEventsBlock.Events[keyEventIndex];

                if ((rftsEvent.EventTypes & RftsEventTypes.IsNew) != 0)
                    yield return BuildAccidentAsNewEvent(rftsEvent, keyEventIndex, rftsEventsBlock.LevelName);
                if ((rftsEvent.EventTypes & RftsEventTypes.IsFailed) != 0 || (rftsEvent.EventTypes & RftsEventTypes.IsFiberBreak) != 0)
                    foreach (var opticalAccidentType in rftsEvent.GetOpticalTypesOfAccident())
                    {
                        var accident = BuildAccidentInOldEvent(rftsEvent, opticalAccidentType, keyEventIndex, rftsEventsBlock.LevelName);
                        accident.OpticalTypeOfAccident = opticalAccidentType;
                        yield return accident;
                    }
            }

            if ((rftsEventsBlock.EELD.Type & ShortDeviationTypes.IsExceeded) != 0)
            {
                yield return new AccidentOnTraceV2()
                {
                    AccidentSeriousness = rftsEventsBlock.LevelName.ConvertToFiberState(),
                    OpticalTypeOfAccident = OpticalAccidentType.TotalLoss,
                    IsAccidentInOldEvent = true,
                    Left = new AccidentNeighbour() { Title = _nodesExcludingAdjustmentPoints.First().Title },
                    Right = new AccidentNeighbour() { Title = _nodesExcludingAdjustmentPoints.Last().Title },
                };
            }
        }

        private AccidentOnTraceV2 BuildAccidentInOldEvent(RftsEvent rftsEvent, 
            OpticalAccidentType opticalAccidentTypeInRftsEvent, int keyEventIndex, RftsLevelType level)
        {
            var brokenLandmarkIndex = _sorData.GetLandmarkIndexForKeyEventIndex(keyEventIndex);
            if (brokenLandmarkIndex == -1)
            {
                // event was not bound to landmark and now it gets worse
                return BuildAccidentAsNewEvent(rftsEvent, keyEventIndex, level);
            }

            var accidentOnTraceV2 = opticalAccidentTypeInRftsEvent == OpticalAccidentType.LossCoeff
                ? GetLossCoeffAccident(rftsEvent, keyEventIndex, level, brokenLandmarkIndex)
                : new AccidentOnTraceV2
                {
                    IsAccidentInOldEvent = true,
                    BrokenRftsEventNumber = keyEventIndex + 1,
                    IsAccidentInLastNode = brokenLandmarkIndex == _nodesExcludingAdjustmentPoints.Count - 1,

                    AccidentLandmarkIndex = brokenLandmarkIndex,
                    AccidentCoors = _nodesExcludingAdjustmentPoints[brokenLandmarkIndex].Position,
                    AccidentToRtuOpticalDistanceKm = _sorData.KeyEventDistanceKm(keyEventIndex),
                    AccidentTitle = GetTitleForLandmark(brokenLandmarkIndex),

                    Left = brokenLandmarkIndex == 0 ? null : GetNeighbour(brokenLandmarkIndex - 1),
                    Right = brokenLandmarkIndex == _sorData.LinkParameters.LandmarksCount - 1 ? null : GetNeighbour(brokenLandmarkIndex + 1),

                    AccidentSeriousness = (rftsEvent.EventTypes & RftsEventTypes.IsFiberBreak) != 0 ? FiberState.FiberBreak : level.ConvertToFiberState(),
                    OpticalTypeOfAccident = opticalAccidentTypeInRftsEvent,
                    EventCode = _sorData.KeyEvents.KeyEvents[keyEventIndex].EventCode.EventCodeForTable(),
                };

            accidentOnTraceV2.DeltaLen = _sorData.GetDeltaLen(accidentOnTraceV2.EventCode[0]);
            return accidentOnTraceV2;
        }

        private AccidentOnTraceV2 GetLossCoeffAccident
            (RftsEvent rftsEvent, int keyEventIndex, RftsLevelType level, int brokenLandmarkIndex)
        {
            if (keyEventIndex == 0)
            {
                _logFile.AppendLine(@"Loss coeff accident could not happen in RTU!");
                keyEventIndex = 1;
            }
            var landmarkIndexToTheLeft = _sorData.GetLandmarkIndexForKeyEventIndex(keyEventIndex - 1);

            return new AccidentOnTraceV2()
            {
                IsAccidentInOldEvent = true,
                BrokenRftsEventNumber = keyEventIndex,
                IsAccidentInLastNode = brokenLandmarkIndex == _nodesExcludingAdjustmentPoints.Count - 1, // always false

                AccidentLandmarkIndex = brokenLandmarkIndex,
                AccidentCoors = _nodesExcludingAdjustmentPoints[brokenLandmarkIndex].Position,
                AccidentToRtuOpticalDistanceKm = _sorData.KeyEventDistanceKm(keyEventIndex),
                AccidentTitle = GetTitleForLandmark(brokenLandmarkIndex),

                Left = GetNeighbour(landmarkIndexToTheLeft),
                Right = GetNeighbour(brokenLandmarkIndex),

                AccidentSeriousness = level.ConvertToFiberState(),
                OpticalTypeOfAccident = rftsEvent.GetOpticalTypeOfAccident(),
                EventCode = _sorData.KeyEvents.KeyEvents[keyEventIndex].EventCode.EventCodeForTable(),
            };
        }

        private AccidentOnTraceV2 BuildAccidentAsNewEvent(RftsEvent rftsEvent, int keyEventIndex, RftsLevelType level)
        {
            var leftLandmarkIndex = _sorData.GetLandmarkToTheLeftFromOwt(_sorData.KeyEvents.KeyEvents[keyEventIndex].EventPropagationTime);
            var rightLandmarkIndex = leftLandmarkIndex + 1;

            var accidentAsNewEvent = new AccidentOnTraceV2()
            {
                IsAccidentInOldEvent = false,
                BrokenRftsEventNumber = keyEventIndex + 1, // keyEventIndex - index, keyEventIndex+1 number

                AccidentLandmarkIndex = -1, // new event
                AccidentToRtuOpticalDistanceKm = _sorData.KeyEventDistanceKm(keyEventIndex),
                Left = GetNeighbour(leftLandmarkIndex),
                Right = GetNeighbour(rightLandmarkIndex),

                AccidentSeriousness = (rftsEvent.EventTypes & RftsEventTypes.IsFiberBreak) != 0 ? FiberState.FiberBreak : level.ConvertToFiberState(),
                OpticalTypeOfAccident = rftsEvent.GetOpticalTypeOfAccident(),
                EventCode = _sorData.KeyEvents.KeyEvents[keyEventIndex].EventCode.EventCodeForTable(),
            };
            accidentAsNewEvent.DeltaLen = _sorData.GetDeltaLen(accidentAsNewEvent.EventCode[0]);
            accidentAsNewEvent.AccidentToLeftOpticalDistanceKm =
                accidentAsNewEvent.AccidentToRtuOpticalDistanceKm - accidentAsNewEvent.Left.ToRtuOpticalDistanceKm;
            accidentAsNewEvent.AccidentToRightOpticalDistanceKm =
               accidentAsNewEvent.Right.ToRtuOpticalDistanceKm - accidentAsNewEvent.AccidentToRtuOpticalDistanceKm;
            _accidentPlaceLocator.SetAccidentInNewEventGps(accidentAsNewEvent, _traceId);
            return accidentAsNewEvent;
        }

        private AccidentNeighbour GetNeighbour(int landmarkIndex)
        {
            var lm = _sorData.LinkParameters.LandmarkBlocks[landmarkIndex];
            var node = _nodesExcludingAdjustmentPoints[landmarkIndex];

            var accidentNeighbour = new AccidentNeighbour()
            {
                LandmarkIndex = landmarkIndex,
                Title = GetTitleForLandmark(landmarkIndex),
                ToRtuOpticalDistanceKm = _sorData.OwtToLenKm(lm.Location),
                Coors = node.Position,
            };
            return accidentNeighbour;
        }

        private string GetTitleForLandmark(int landmarkIndex)
        {
            if (landmarkIndex == 0) return _rtu.Title;

            var node = _nodesExcludingAdjustmentPoints[landmarkIndex];
            var equipment = _equipmentsWithoutPointsAndRtu[landmarkIndex - 1];

            var title = node.Title;
            if (equipment.Type != EquipmentType.EmptyNode && !string.IsNullOrEmpty(equipment.Title))
                title = title + @" / " + equipment.Title;
            return title;
        }
    }
}