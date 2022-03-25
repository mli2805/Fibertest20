using System;
using System.Collections.Generic;
using System.Linq;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public static class RftsParamsMapper
    {
        public static RftsParametersModel ToModel(this RftsParams rftsParams)
        {
            return new RftsParametersModel
            {
                Levels = rftsParams.Levels.Select(l => l.ToModel()).ToList(),
                UniParams = rftsParams.UniParams.Select(u => u.ToModel()).ToList()
            };
        }

        private static RftsParamsLevelModel ToModel(this RftsParamsLevel level)
        {
            Enum.TryParse(level.LevelName, true, out FiberState levelName);
            return new RftsParamsLevelModel()
            {
                LevelName = levelName.ToLocalizedString(),
                IsEnabled = level.Enabled,

                Lines = new List<ThresholdLine>()
                {
                    level.LevelThresholdSet.Lt.ToModel(@"Lt", Resources.SID_Lt_threshold),
                    level.LevelThresholdSet.Rt.ToModel(@"Rt", Resources.SID_Rt_threshold),
                    level.LevelThresholdSet.Ct.ToModel(@"Ct", Resources.SID_Ct_threshold),
                    level.Eelt.ToModel(@"Eelt", Resources.SID_Eelt_threshold),
                }
            };
        }

        private static ThresholdLine ToModel(this Threshold threshold, string code, string title)
        {
            return new ThresholdLine()
            {
                Code = code,
                Title = title,
                IsRelative = !threshold.Absolute,
                Value = threshold.Absolute ? threshold.AbsoluteThreshold / 1000.0 : threshold.RelativeThreshold / 1000.0
            };
        }

        private static RftsUniParamModel ToModel(this RftsUniParameter uniParameter)
        {
            return new RftsUniParamModel
            {
                Name = uniParameter.Name.GetUniParamLocalizedName(),
                Value = (double)uniParameter.Value / uniParameter.Scale
            };
        }

        private static string GetUniParamLocalizedName(this string name)
        {
            switch (name)
            {
                case "EvtDetectDeltaCT" : return Resources.SID_EvtDetectDeltaCT;
                case "EvtSearchStep" : return Resources.SID_EvtSearchStep;
                case "EvtDetectDeltaLen": return Resources.SID_EvtDetectDeltaLen;
                case "EvtRDetectDeltaLen": return Resources.SID_EvtRDetectDeltaLen;
                case "AutoLT": return Resources.SID_AutoLT;
                case "AutoRT": return Resources.SID_AutoRT;
                case "AutoET": return Resources.SID_AutoET;
                case "NoLinkDistance": return Resources.SID_NoLinkDistance;
                case "NoLinkDeltaDB": return Resources.SID_NoLinkDeltaDB;
                case "EventRT": return Resources.SID_EventRT;
                case "EOFLMN": return Resources.SID_EOFLMN;
                case "CorrectMarkers": return Resources.SID_CorrectMarkers;
                case "EvtChangeLT": return Resources.SID_EvtChangeLT;
                case "EvtChangeRT": return Resources.SID_EvtChangeRT;
                case "EvtChangeCT": return Resources.SID_EvtChangeCT;
                case "EvtChangeET": return Resources.SID_EvtChangeET;
                default: return @"unknown";
            }
        }
    }
}
