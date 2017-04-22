using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using IitOtdrLibrary;
using Optixsoft.SharedCommons.SorSerialization;
using Optixsoft.SorExaminer.OtdrDataFormat;
using Optixsoft.SorExaminer.OtdrDataFormat.Structures;
using BinaryReader = Optixsoft.SharedCommons.SorSerialization.BinaryReader;

namespace RtuWpfExample
{
    public class SorDataParser
    {
        private readonly OtdrDataKnownBlocks _sorData;
        private readonly OtdrDataKnownBlocks _baseSorData;
        private int _eventCount;

        private Dictionary<int, string> LineNameList => new Dictionary<int, string>
        {
            { 100, "       Common Information"       },
            { 101, "Landmark Name"                   },
            { 102, "Landmark Type"                   },
            { 103, "State"                           },
            { 104, "Damage Type"                     },
            { 105, "Distance, km"                    },
            { 106, "Enabled"                         },
            { 107, "Event Type"                      },
            { 200, "       Current Measurement"      },
            { 201, "Reflectance coefficient, dB"     },
            { 202, "Attenuation in Closure, dB"      },
            { 203, "Attenuation coefficient, dB/km " },
            { 300, "       Monitoring Thresholds"    },
            { 301, "Reflectance coefficient, dB"     },
            { 302, "Attenuation in Closure, dB"      },
            { 303, "Attenuation coefficient, dB/km " },
            { 400, "       Deviations from Base"     },
            { 401, "Reflectance coefficient, dB"     },
            { 402, "Attenuation in Closure, dB"      },
            { 403, "Attenuation coefficient, dB/km " },
            { 900, ""                                },
        };

        public SorDataParser(OtdrDataKnownBlocks sorData)
        {
            _sorData = sorData;
            _baseSorData = ExtractBase();
        }

        public EventsContent Parse(RftsLevelType rftsLevel)
        {
            _eventCount = _sorData.LinkParameters.LandmarksCount;
            var eventsContent = PrepareEmptyDictionary();

            ParseCommonInformation(eventsContent.Table);
            ParseCurrentMeasurement(eventsContent.Table);
            ParseMonitoringThresholds(eventsContent.Table, rftsLevel);
            var rftsEvents = ExtractRftsEventsForLevel(rftsLevel);
            if (rftsEvents != null)
                ParseDeviationFromBase(eventsContent, rftsEvents);
            SetEventStates(eventsContent.Table);

            return eventsContent;
        }

        private OtdrDataKnownBlocks ExtractBase()
        {
            if (_sorData.EmbeddedData.EmbeddedBlocksCount == 0 ||
                _sorData.EmbeddedData.EmbeddedDataBlocks[0].Description != "SOR")
                return null;

            var bytes = _sorData.EmbeddedData.EmbeddedDataBlocks[0].Data;
            var baseSorData = SorData.FromBytes(bytes);
            return baseSorData;
        }

        private RftsEventsBlock ExtractRftsEventsForLevel(RftsLevelType rftsLevel)
        {
            for (int i = 0; i < _sorData.EmbeddedData.EmbeddedBlocksCount; i++)
            {
                if (_sorData.EmbeddedData.EmbeddedDataBlocks[i].Description != "RFTSEVENTS")
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

        private EventsContent PrepareEmptyDictionary()
        {
            var eventsContent = new EventsContent();
            foreach (var pair in LineNameList)
            {
                var cells = new string[_eventCount + 1];
                cells[0] = pair.Value;
                eventsContent.Table.Add(pair.Key, cells);
            }
            return eventsContent;
        }

        private void ParseCommonInformation(Dictionary<int, string[]> eventTable)
        {
            for (int i = 0; i < _eventCount; i++)
            {
                eventTable[101][i + 1] = _sorData.LinkParameters.LandmarkBlocks[i].Comment;
                eventTable[102][i + 1] = _sorData.LinkParameters.LandmarkBlocks[i].Code.ForTable();
                eventTable[105][i + 1] = $"{OwtToLen(_sorData.KeyEvents.KeyEvents[i].EventPropagationTime): 0.00000}";
                eventTable[106][i + 1] = _sorData.RftsEvents.Events[i].EventTypes.ForTable();
                eventTable[107][i + 1] = EventCodeForTable(_sorData.KeyEvents.KeyEvents[i].EventCode);
            }
        }

        private string EventCodeForTable(string eventCode)
        {
            var str = eventCode[0] == '0' ? "S" : "R";
            return $"{str} : {eventCode[1]}";
        }

        private double OwtToLen(int owt)
        {
            var owt1 = owt - _sorData.GeneralParameters.UserOffset;
            return owt1 * OwtToKmCoeff;
        }

        const double LightSpeed = 0.000299792458; // km/ns
        private double OwtToKmCoeff => LightSpeed / _sorData.FixedParameters.RefractionIndex / 10;

        private double F(double p)
        {
            return p / OwtToKmCoeff;
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
                eventTable[202][i + 1] = eventLoss > endOfFiberThreshold ? $">{endOfFiberThreshold:0.000}" : $"{eventLoss:0.000}";
                eventTable[203][i + 1] = $"{F(_sorData.KeyEvents.KeyEvents[i].LeadInFiberAttenuationCoefficient) : 0.000}";
            }
        }

        private void ParseMonitoringThresholds(Dictionary<int, string[]> eventTable, RftsLevelType rftsLevel)
        {
            var level = _sorData.RftsParameters.Levels.First(l => l.LevelName == rftsLevel);

            for (int i = 0; i < _eventCount; i++)
            {
                eventTable[301][i + 1] = level.ThresholdSets[i].ReflectanceThreshold.ForTable();
                eventTable[302][i + 1] = level.ThresholdSets[i].AttenuationThreshold.ForTable();
                eventTable[303][i + 1] = level.ThresholdSets[i].AttenuationCoefThreshold.ForTable();
            }
        }

        private void ParseDeviationFromBase(EventsContent eventsContent, RftsEventsBlock rftsEvents)
        {
            for (int i = 0; i < _eventCount; i++)
            {
                eventsContent.Table[401][i + 1] = ForTable(eventsContent, rftsEvents.Events[i].ReflectanceThreshold, i+1, "R");
                if (i < _eventCount-1)
                    eventsContent.Table[402][i + 1] = ForTable(eventsContent, rftsEvents.Events[i].AttenuationThreshold, i+1, "L");
                eventsContent.Table[403][i + 1] = ForTable(eventsContent, rftsEvents.Events[i].AttenuationCoefThreshold, i+1, "C");
            }
        }

        private string ForTable(EventsContent eventsContent, ShortDeviation deviation, int column, string letter)
        {
            var formattedValue = $"{deviation.Deviation / 1000.0: 0.000}";
            if ((deviation.Type & ShortDeviationTypes.IsExceeded) != 0)
            {
                formattedValue += $" ( {letter} ) ";
                eventsContent.Table[104][column] += $" {letter}";
                eventsContent.IsFailed = true;
            }
            return formattedValue;
        }

        private void SetEventStates(Dictionary<int, string[]> eventTable)
        {
            for (int i = 0; i < _eventCount; i++)
            {
                eventTable[103][i + 1] = string.IsNullOrEmpty(eventTable[104][i + 1]) ? "pass" : "fail";
            }
        }

    }
}