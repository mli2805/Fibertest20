using System.Collections.Generic;
using System.Linq;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Optixsoft.SorExaminer.OtdrDataFormat;
using Optixsoft.SorExaminer.OtdrDataFormat.Structures;

namespace Iit.Fibertest.D2RtuVeexLibrary
{
    public static class SorDataThresholdExtractor
    {
        public static ThresholdSet ExtractThresholds(this byte[] bytes)
        {
            if (SorData.TryGetFromBytes(bytes, out OtdrDataKnownBlocks sorData) != "")
                return null;

            var thresholdSet = new ThresholdSet() { Levels = new List<Level>() };
            thresholdSet.AddRftsLevel(sorData, "Critical");
            thresholdSet.AddRftsLevel(sorData, "Major");
            thresholdSet.AddRftsLevel(sorData, "Minor");

            return thresholdSet;
        }

        private static void AddRftsLevel(this ThresholdSet thresholdSet, OtdrDataKnownBlocks sorData, string levelName)
        {
            var rftsParametersLevel =
                sorData.RftsParameters.Levels.FirstOrDefault(l => l.LevelName.ToString() == levelName);
            if (rftsParametersLevel == null) return;
            var level = new Level()
            {
                Name = rftsParametersLevel.LevelName.ToString(), 
                Groups = new List<Group>(){ new Group() { Thresholds = GetLevelThresholds(rftsParametersLevel, sorData) }},
                AdvancedThresholds = GetAdvancedThresholds(sorData),
            };
            thresholdSet.Levels.Add(level);
        }

        private static Thresholds GetLevelThresholds(RftsLevel levelParams, OtdrDataKnownBlocks sorData)
        {
            return new Thresholds()
            {
                EventReflectance = Iit2CmbThreshold(levelParams.LevelThresholdSet.ReflectanceThreshold),
                EventLoss = Iit2CmbThreshold(levelParams.LevelThresholdSet.AttenuationThreshold),
                EventLeadingLossCoefficient =
                    Iit2CmbThreshold(levelParams.LevelThresholdSet.AttenuationCoefThreshold),
                EventMaxLevel = null, // PON, reflect do not work with this parameter

                ReflectiveEventPosition = IitUniversalParam2CmbThreshold(sorData, "EvtRDetectDeltaLen"),
                NonReflectiveEventPosition = IitUniversalParam2CmbThreshold(sorData, "EvtDetectDeltaLen"),
            };
        }

        private static AdvancedThresholds GetAdvancedThresholds(OtdrDataKnownBlocks sorData)
        {
            return new AdvancedThresholds()
            {
                AttenuationCoefficientChangeForNewEvents = IitUniversalParam2double(sorData, "EvtDetectDeltaCT"),
                EofAttenuationCoefficientChangeForFiberBreak = IitUniversalParam2double(sorData, "EvtChangeСT"),
                EofLossChangeForFiberBreak = IitUniversalParam2double(sorData, "EvtChangeЕT"),
                MaxEofAttenuationCoefficientForFiberBreak = 0.05,
                // noiseLevelChangeForFiberElongation, // is not used in IIT
            };
        }

        private static double IitUniversalParam2double(OtdrDataKnownBlocks sorData, string paramName)
        {
            var parameter = sorData.RftsParameters.UniversalParameters.FirstOrDefault(p => p.Name == paramName);
            if (parameter == null) return 0;

            return (double)parameter.Value / parameter.Scale;
        }

        private static CombinedThreshold IitUniversalParam2CmbThreshold(OtdrDataKnownBlocks sorData, string paramName)
        {
            var parameter = sorData.RftsParameters.UniversalParameters.FirstOrDefault(p => p.Name == paramName);
            if (parameter == null) return null;

            return new CombinedThreshold()
            {
                Min = - parameter.Value / parameter.Scale,
                Max = parameter.Value / parameter.Scale,
            };
        }

        private static CombinedThreshold Iit2CmbThreshold(ShortThreshold iitThreshold)
        {
            // ShortThreshold contains int values multiplied by 1000
            const double iitCoef = 1000;

            var cmbThreshold = new CombinedThreshold();
            if (iitThreshold.IsAbsolute)
            {
                cmbThreshold.Min = - iitThreshold.AbsoluteThreshold / iitCoef;
                cmbThreshold.Max = iitThreshold.AbsoluteThreshold / iitCoef;
            }
            else
            {
                cmbThreshold.Increase = iitThreshold.RelativeThreshold / iitCoef;
                cmbThreshold.Decrease = -iitThreshold.RelativeThreshold / iitCoef;
            }
            return cmbThreshold;
        }
    }
}
