﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Iit.Fibertest.Client
{
    public static class RftsTemplateExt
    {
        private const int LinesForShortThreshold = 6;
        private const int LinesForRftsUniversalParameter = 6;
        private const int LinesForRftsLevelThresholdSet = 3 * LinesForShortThreshold;
        private const int LinesForRftsLevel = 4 + LinesForRftsLevelThresholdSet + LinesForShortThreshold;

        public static bool TryLoad(string filename, out RftsParams result, out Exception exception)
        {
            try
            {
                var lines = File.ReadAllLines(filename).ToList();
                result = ParseRftsParams(lines);
                exception = null;
                return true;
            }
            catch (Exception e)
            {
                result = null;
                exception = e;
                return false;
            }
        }

        private static RftsParams ParseRftsParams(List<string> lines)
        {
            var result = new RftsParams();
            result.LevelNumber = int.Parse(lines[1]);

            for (int i = 0; i < result.LevelNumber; i++)
            {
                var level = ParseRftsLevel(lines.GetRange(2 + i * LinesForRftsLevel, (i + 1) * LinesForRftsLevel));
                result.Levels.Add(level);
            }

            var uniParamStart = 2 + 3 * LinesForRftsLevel;
            result.UniversalParamNumber = int.Parse(lines[uniParamStart + 1]);
            for (int i = 0; i < result.UniversalParamNumber; i++)
            {
                var uniParam = ParseRftsUniversalParameter(
                    lines.GetRange(uniParamStart + 2 + i * LinesForRftsUniversalParameter, LinesForRftsUniversalParameter));
                result.UniParams.Add(uniParam);
            }

            return result;
        }

        private static RftsLevel ParseRftsLevel(List<string> lines)
        {
            var level = new RftsLevel();
            level.LevelName = lines[1];
            var enabled = int.Parse(lines[3]);
            level.Enabled = enabled != 0;
            level.LevelThresholdSet = ParseThresholdSet(lines.GetRange(4, LinesForRftsLevelThresholdSet));
            level.Eelt = ParseShortThreshold(lines.GetRange(4 + LinesForRftsLevelThresholdSet, LinesForShortThreshold));
            return level;
        }

        private static RftsLevelThresholdSet ParseThresholdSet(List<string> lines)
        {
            var thresholdSet = new RftsLevelThresholdSet();

            thresholdSet.Lt = ParseShortThreshold(lines.GetRange(0 * LinesForShortThreshold, LinesForShortThreshold));
            thresholdSet.Rt = ParseShortThreshold(lines.GetRange(1 * LinesForShortThreshold, LinesForShortThreshold));
            thresholdSet.Ct = ParseShortThreshold(lines.GetRange(2 * LinesForShortThreshold, LinesForShortThreshold));

            return thresholdSet;
        }

        private static ShortThreshold ParseShortThreshold(List<string> lines)
        {
            var shortThreshold = new ShortThreshold();

            var absolute = int.Parse(lines[1]);
            shortThreshold.Absolute = absolute != 0;
            shortThreshold.AbsoluteThreshold = int.Parse(lines[3]);
            shortThreshold.RelativeThreshold = int.Parse(lines[5]);

            return shortThreshold;
        }

        private static RftsUniversalParameter ParseRftsUniversalParameter(List<string> lines)
        {
            var uniParam = new RftsUniversalParameter();

            uniParam.Name = lines[1];
            uniParam.Value = int.Parse(lines[3]);
            uniParam.Scale = int.Parse(lines[5]);

            return uniParam;
        }
    }
}
