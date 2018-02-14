using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Iit.Fibertest.IitOtdrLibrary;
using Iit.Fibertest.StringResources;
using Optixsoft.SharedCommons.SorSerialization;
using Optixsoft.SorExaminer.OtdrDataFormat;
using Optixsoft.SorExaminer.OtdrDataFormat.Structures;
using BinaryReader = Optixsoft.SharedCommons.SorSerialization.BinaryReader;

namespace Iit.Fibertest.WpfCommonViews
{
    public class SorDataToViewContent
    {
        private readonly OtdrDataKnownBlocks _sorData;
        private int _eventCount;

        private Dictionary<int, string> LineNameList => new Dictionary<int, string>
        {
            { 100, Resources.SID________Common_Information       },
            { 101, Resources.SID_Landmark_Name                   },
            { 102, Resources.SID_Landmark_Type                   },
            { 103, Resources.SID_State                           },
            { 104, Resources.SID_Damage_Type                     },
            { 105, Resources.SID_Distance__km                    },
            { 106, Resources.SID_Enabled                         },
            { 107, Resources.SID_Event_Type                      },
            { 200, Resources.SID________Current_Measurement      },
            { 201, Resources.SID_Reflectance_coefficient__dB     },
            { 202, Resources.SID_Attenuation_in_Closure__dB      },
            { 203, Resources.SID_Attenuation_coefficient__dB_km_ },
            { 300, Resources.SID________Monitoring_Thresholds    },
            { 301, Resources.SID_Reflectance_coefficient__dB     },
            { 302, Resources.SID_Attenuation_in_Closure__dB      },
            { 303, Resources.SID_Attenuation_coefficient__dB_km_ },
            { 400, Resources.SID________Deviations_from_Base     },
            { 401, Resources.SID_Reflectance_coefficient__dB     },
            { 402, Resources.SID_Attenuation_in_Closure__dB      },
            { 403, Resources.SID_Attenuation_coefficient__dB_km_ },
            { 900, ""                                            },
        };

        public SorDataToViewContent(OtdrDataKnownBlocks sorData)
        {
            _sorData = sorData;
        }

        public OneLevelTableContent Parse(RftsLevelType rftsLevel)
        {
            _eventCount = _sorData.RftsEvents.EventsCount;
            var eventContent = PrepareEmptyDictionary();

            ParseCommonInformation(eventContent);
            ParseCurrentMeasurement(eventContent.Table);
            ParseMonitoringThresholds(eventContent.Table, rftsLevel);
            var rftsEvents = ExtractRftsEventsForLevel(rftsLevel);
            if (rftsEvents != null)
                ParseDeviationFromBase(eventContent, rftsEvents);

            return eventContent;
        }

        private RftsEventsBlock ExtractRftsEventsForLevel(RftsLevelType rftsLevel)
        {
            for (int i = 0; i < _sorData.EmbeddedData.EmbeddedBlocksCount; i++)
            {
                if (_sorData.EmbeddedData.EmbeddedDataBlocks[i].Description != @"RFTSEVENTS")
                    continue;

                var bytes = _sorData.EmbeddedData.EmbeddedDataBlocks[i].Data;
                var binaryReader = new BinaryReader(new System.IO.BinaryReader(new MemoryStream(bytes)));
                ushort revision = binaryReader.ReadUInt16();
                var opxDeserializer = new OpxDeserializer(binaryReader, revision);
                var result = (RftsEventsBlock)opxDeserializer.Deserialize(typeof(RftsEventsBlock));
                if (result.LevelName == rftsLevel)
                    return result;
            }
            return null;
        }

        private OneLevelTableContent PrepareEmptyDictionary()
        {
            var eventsContent = new OneLevelTableContent();
            foreach (var pair in LineNameList)
            {
                var cells = new string[_eventCount + 1];
                cells[0] = pair.Value;
                eventsContent.Table.Add(pair.Key, cells);
            }
            return eventsContent;
        }

        private void ParseCommonInformation(OneLevelTableContent oneLevelTableContent)
        {
            for (int i = 0; i < _eventCount; i++)
            {
                var landmark = _sorData.LinkParameters.LandmarkBlocks.FirstOrDefault(b => b.RelatedEventNumber == i + 1);
                if (landmark != null)
                {
                    oneLevelTableContent.Table[101][i + 1] = landmark.Comment;
                    oneLevelTableContent.Table[102][i + 1] = landmark.Code.ForTable();
                }

                oneLevelTableContent.Table[103][i + 1] = _sorData.RftsEvents.Events[i].EventTypes.ForStateInTable();
                oneLevelTableContent.Table[105][i + 1] = $@"{_sorData.OwtToLenKm(_sorData.KeyEvents.KeyEvents[i].EventPropagationTime):0.00000}";
                if ((_sorData.RftsEvents.Events[i].EventTypes & RftsEventTypes.IsNew) != 0)
                {
                    oneLevelTableContent.IsFailed = true;
                    if (string.IsNullOrEmpty(oneLevelTableContent.FirstProblemLocation))
                        oneLevelTableContent.FirstProblemLocation = oneLevelTableContent.Table[105][i + 1];
                    oneLevelTableContent.Table[105][i + 1] += Resources.SID___new_;
                }
                oneLevelTableContent.Table[106][i + 1] = _sorData.RftsEvents.Events[i].EventTypes.ForEnabledInTable();
                oneLevelTableContent.Table[107][i + 1] = _sorData.KeyEvents.KeyEvents[i].EventCode.EventCodeForTable();
            }
        }

        private double AttenuationCoeffToDbKm(double p)
        {
            return p / _sorData.GetOwtToKmCoeff();
        }
        private void ParseCurrentMeasurement(Dictionary<int, string[]> eventTable)
        {
            for (int i = 0; i < _eventCount; i++)
            {
                eventTable[201][i + 1] = _sorData.KeyEvents.KeyEvents[i].EventReflectance.ToString(CultureInfo.CurrentCulture);
                if (i == 0)
                    continue;
                var eventLoss = _sorData.KeyEvents.KeyEvents[i].EventLoss;
                var endOfFiberThreshold = _sorData.FixedParameters.EndOfFiberThreshold;
                eventTable[202][i + 1] = eventLoss > endOfFiberThreshold ? $@">{endOfFiberThreshold:0.000}" : $@"{eventLoss:0.000}";
                eventTable[203][i + 1] = $@"{AttenuationCoeffToDbKm(_sorData.KeyEvents.KeyEvents[i].LeadInFiberAttenuationCoefficient): 0.000}";
            }
        }

        private void ParseMonitoringThresholds(Dictionary<int, string[]> eventTable, RftsLevelType rftsLevel)
        {
            var level = _sorData.RftsParameters.Levels.First(l => l.LevelName == rftsLevel);

            for (int i = 0; i < _eventCount; i++)
            {
                if ((_sorData.RftsEvents.Events[i].EventTypes & RftsEventTypes.IsNew) != 0)
                    continue;
                eventTable[301][i + 1] = level.ThresholdSets[i].ReflectanceThreshold.ForTable();
                eventTable[302][i + 1] = level.ThresholdSets[i].AttenuationThreshold.ForTable();
                eventTable[303][i + 1] = level.ThresholdSets[i].AttenuationCoefThreshold.ForTable();
            }
        }

        private void ParseDeviationFromBase(OneLevelTableContent oneLevelTableContent, RftsEventsBlock rftsEvents)
        {
            for (int i = 0; i < _eventCount; i++)
            {
                if ((rftsEvents.Events[i].EventTypes & RftsEventTypes.IsFiberBreak) != 0)
                {
                    oneLevelTableContent.IsFailed = true;
                    oneLevelTableContent.Table[104][i + 1] = @"B";
                    if (string.IsNullOrEmpty(oneLevelTableContent.FirstProblemLocation))
                        oneLevelTableContent.FirstProblemLocation = oneLevelTableContent.Table[105][i + 1];
                }
                oneLevelTableContent.Table[401][i + 1] = ForDeviationInTable(oneLevelTableContent, rftsEvents.Events[i].ReflectanceThreshold, i + 1, @"R");
                if (i < _eventCount - 1)
                    oneLevelTableContent.Table[402][i + 1] = ForDeviationInTable(oneLevelTableContent, rftsEvents.Events[i].AttenuationThreshold, i + 1, @"L");
                oneLevelTableContent.Table[403][i + 1] = ForDeviationInTable(oneLevelTableContent, rftsEvents.Events[i].AttenuationCoefThreshold, i + 1, @"C");
            }
        }

        private string ForDeviationInTable(OneLevelTableContent oneLevelTableContent, ShortDeviation deviation, int column, string letter)
        {
            var formattedValue = $@"{(short)deviation.Deviation / 1000.0: 0.000}";
            if ((deviation.Type & ShortDeviationTypes.IsExceeded) != 0)
            {
                formattedValue += $@" ( {letter} ) ";
                oneLevelTableContent.Table[104][column] += $@" {letter}";
                oneLevelTableContent.IsFailed = true;
                if (string.IsNullOrEmpty(oneLevelTableContent.FirstProblemLocation))
                    oneLevelTableContent.FirstProblemLocation = oneLevelTableContent.Table[105][column];
            }
            return formattedValue;
        }

        //        private OtdrDataKnownBlocks ExtractBase()
        //        {
        //            if (_sorData.EmbeddedData.EmbeddedBlocksCount == 0 ||
        //                _sorData.EmbeddedData.EmbeddedDataBlocks[0].Description != @"SOR")
        //                return null;
        //
        //            var bytes = _sorData.EmbeddedData.EmbeddedDataBlocks[0].Data;
        //            var baseSorData = SorBytes.FromBytes(bytes);
        //            return baseSorData;
        //        }
    }
}