using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Optixsoft.SharedCommons.SorSerialization;
using Optixsoft.SorExaminer.OtdrDataFormat;
using Optixsoft.SorExaminer.OtdrDataFormat.Structures;
using BinaryReader = Optixsoft.SharedCommons.SorSerialization.BinaryReader;

namespace RtuWpfExample
{
    public class SorDataParser
    {
        private readonly OtdrDataKnownBlocks _sorData;
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
        }


        public Dictionary<int, string[]> Parse(RftsLevelType rftsLevel)
        {
            _eventCount = _sorData.LinkParameters.LandmarksCount;
            var eventTable = PrepareEmptyDictionary();

            ParseCommonInformation(eventTable);
            ParseCurrentMeasurement(eventTable);
            ParseMonitoringThresholds(eventTable, rftsLevel);
            var rftsEvents = ExtractRftsEventsForLevel(rftsLevel);
            if (rftsEvents != null)
                ParseDeviationFromBase(eventTable, rftsEvents);
            return eventTable;
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

        private Dictionary<int, string[]> PrepareEmptyDictionary()
        {
            var eventTable = new Dictionary<int, string[]>();
            foreach (var pair in LineNameList)
            {
                var cells = new string[_eventCount + 1];
                cells[0] = pair.Value;
                eventTable.Add(pair.Key, cells);
            }
            return eventTable;
        }

        private void ParseCommonInformation(Dictionary<int, string[]> eventTable)
        {
            for (int i = 0; i < _eventCount; i++)
            {
                eventTable[101][i + 1] = _sorData.LinkParameters.LandmarkBlocks[i].Comment;
                eventTable[102][i + 1] = _sorData.LinkParameters.LandmarkBlocks[i].Code.ForTable();
                eventTable[105][i + 1] = $"{OwtToLen(_sorData.KeyEvents.KeyEvents[i].EventPropagationTime) : 0.00000}";
                eventTable[106][i + 1] = _sorData.RftsEvents.Events[i].EventTypes.ForTable();
            }
        }

        private double OwtToLen(int owt)
        {
            var owt1 = owt - _sorData.GeneralParameters.UserOffset;
            const double lightSpeed = 0.000299792458; // km/ns
            var ri = _sorData.FixedParameters.RefractionIndex;
            return owt1 * lightSpeed / ri / 10;
        }
        private void ParseCurrentMeasurement(Dictionary<int, string[]> eventTable)
        {
            for (int i = 0; i < _eventCount; i++)
            {
                eventTable[201][i + 1] = _sorData.KeyEvents.KeyEvents[i].EventReflectance.ToString(CultureInfo.CurrentCulture);
                eventTable[202][i + 1] = _sorData.KeyEvents.KeyEvents[i].EventLoss.ToString(CultureInfo.CurrentCulture);
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

        private void ParseDeviationFromBase(Dictionary<int, string[]> eventTable, RftsEventsBlock rftsEvents)
        {
            for (int i = 0; i < _eventCount; i++)
            {
                eventTable[401][i + 1] = $"{(short)rftsEvents.Events[i].ReflectanceThreshold.Deviation / 1000.0 : 0.000}";
                eventTable[402][i + 1] = $"{rftsEvents.Events[i].AttenuationThreshold.Deviation / 1000.0 : 0.000}"; 
                eventTable[403][i + 1] = $"{rftsEvents.Events[i].AttenuationCoefThreshold.Deviation / 1000.0 : 0.000}";
            }
        }


    }
}