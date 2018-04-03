using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Iit.Fibertest.Dto;
using Iit.Fibertest.IitOtdrLibrary;
using Iit.Fibertest.UtilsLib;
using Optixsoft.SorExaminer.OtdrDataFormat;
using Optixsoft.SorExaminer.OtdrDataFormat.Structures;

namespace Iit.Fibertest.Graph.Algorithms
{
    public class AccidentsFromSorExtractor
    {
        private readonly IMyLog _logFile;
        private OtdrDataKnownBlocks _sorData;
        private OtdrDataKnownBlocks _baseSorData;
        private readonly List<string> _report = new List<string>();

        public AccidentsFromSorExtractor(IMyLog logFile)
        {
            _logFile = logFile;
        }

        public List<AccidentOnTrace> GetAccidents(OtdrDataKnownBlocks sorData, bool isClient)
        {
            _sorData = sorData;

            try
            {
                return GetAccidents(isClient);
            }
            catch (Exception e)
            {
                _logFile.AppendLine(@"GetAccidents: " + e.Message);
                return new List<AccidentOnTrace>();
            }
        }

        private List<AccidentOnTrace> GetAccidents(bool isClient)
        {
            _baseSorData = _sorData.GetBase();
            var rftsEventsBlocks = _sorData.GetRftsEventsBlockForEveryLevel().ToList();

            if (isClient) ReportBaseAndMeasEventsParsing(rftsEventsBlocks);

            var levels = new List<RftsLevelType>() {RftsLevelType.Critical, RftsLevelType.Major, RftsLevelType.Minor};
            var result = new List<AccidentOnTrace>();

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
            for (int i = 1; i < rftsEventsBlock.EventsCount; i++) // 0 - RTU
            {
                var rftsEvent = rftsEventsBlock.Events[i];

                if ((rftsEvent.EventTypes & RftsEventTypes.IsNew) != 0)
                    yield return BuildAccidentAsNewEvent(rftsEvent, i, rftsEventsBlock.LevelName);
                if ((rftsEvent.EventTypes & RftsEventTypes.IsFailed) != 0 || (rftsEvent.EventTypes & RftsEventTypes.IsFiberBreak) != 0)
                    foreach (var opticalAccidentType in GetOpticalTypesOfAccident(rftsEvent))
                    {
                        var accident = BuildAccidentInOldEvent(rftsEvent, i, rftsEventsBlock.LevelName);
                        accident.OpticalTypeOfAccident = opticalAccidentType;
                        yield return accident;
                    }
            }
        }
      
        private AccidentOnTrace BuildAccidentInOldEvent(RftsEvent rftsEvent, int i, RftsLevelType level)
        {
            var brokenLandmarkIndex = _sorData.GetLandmarkIndexForKeyEventIndex(i);
            if (brokenLandmarkIndex == -1)
            {
                // event was not bound to landmark and now it gets worse
                return BuildAccidentAsNewEvent(rftsEvent, i, level);
            }
            var previousLandmark = _sorData.LinkParameters.LandmarkBlocks[brokenLandmarkIndex - 1];

            var accidentInOldEvent = new AccidentInOldEvent
            {
                BrokenRftsEventNumber = i + 1, // i - index, i+1 number

                BrokenLandmarkIndex = brokenLandmarkIndex,
                AccidentDistanceKm = _sorData.KeyEventDistanceKm(i),
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
//            else if ((rftsEvent.EventTypes & RftsEventTypes.IsNew) != 0)
//                yield return OpticalAccidentType.Loss;
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

#region Log
        [Localizable(false)]
        private void ReportBaseAndMeasEventsParsing(List<RftsEventsBlock> levels)
        {
            ReportLandmarks(_baseSorData);
            ReportKeyAndRftsEvents(_baseSorData, null);
            ReportLandmarks(_sorData);
            ReportKeyAndRftsEvents(_sorData, levels);

            var tempFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\Temp\");
            if (!Directory.Exists(tempFolder))
                Directory.CreateDirectory(tempFolder);
            var filename = Guid.NewGuid() + ".txt";
            var fullFilename = Path.Combine(tempFolder, filename);
            File.WriteAllLines(fullFilename, _report);
        }

        [Localizable(false)]
        private void ReportLandmarks(OtdrDataKnownBlocks sorData)
        {
            _report.Add("");
            _report.Add("   Landmarks");
            for (int i = 0; i < sorData.LinkParameters.LandmarksCount; i++)
            {
                var lm = sorData.LinkParameters.LandmarkBlocks[i];
                var line = $"index {i}  owt {lm.Location:D6} ({sorData.OwtToLenKm(lm.Location),7:F3} km)";
                if (lm.RelatedEventNumber != 0)
                    line = line + $"  related event Number {lm.RelatedEventNumber}";
                _report.Add(line);
            }
        }


        [Localizable(false)]
        private void ReportKeyAndRftsEvents(OtdrDataKnownBlocks sorData, List<RftsEventsBlock> rftsEventsBlocks)
        {
            var dict = new Dictionary<string, List<string>>();
            if (rftsEventsBlocks != null) // base
                foreach (var rftsEventsForOneLevel in rftsEventsBlocks)
                {
                    var list = rftsEventsForOneLevel.Events.Select(rftsEvent => EventStateToString(rftsEvent.EventTypes)).ToList();
                    dict.Add(rftsEventsForOneLevel.LevelName.ToString(), list);
                }

            _report.Add("");
            _report.Add("    Events (Minor / Major / Critical)");
            for (var i = 0; i < sorData.KeyEvents.KeyEvents.Length; i++)
            {
                var keyEvent = sorData.KeyEvents.KeyEvents[i];
                var line = $@"Number {i + 1}  owt {keyEvent.EventPropagationTime:D6} ({sorData.KeyEventDistanceKm(i),7:F3} km)";
                if (dict.ContainsKey("Minor")) line = line + "   " + $"{dict["Minor"][i],21}";
                if (dict.ContainsKey("Major")) line = line + "   " + $"{dict["Major"][i],21}";
                if (dict.ContainsKey("Critical")) line = line + "   " + $"{dict["Critical"][i],21}";
                _report.Add(line);
            }
            _report.Add("");
        }

        private string EventStateToString(RftsEventTypes state)
        {
            var result = "";
            if ((state & RftsEventTypes.IsNew) != 0) result = result + @" IsNew";
            if ((state & RftsEventTypes.IsFailed) != 0) result = result + @" IsFailed";
            if ((state & RftsEventTypes.IsFiberBreak) != 0) result = result + @" IsFiberBreak";
            if (result == "") result = @" IsMonitored";
            return result;
        }
#endregion
    }
}