using System.Linq;

namespace Iit.Fibertest.Client
{
    public static class ModelToRftsParams
    {
        public static RftsParams ToRftsParams(this RftsParametersModel model)
        {
            var result = new RftsParams()
            {
                Levels = model.Levels.Select(l=>l.ToRftsParams()).ToList(),
                UniParams = model.UniParams.Select(u=>u.ToRftsParams()).ToList(),
            };
            result.LevelNumber = result.Levels.Count;
            result.UniversalParamNumber = result.UniParams.Count;
            return result;
        }

        private static RftsParamsLevel ToRftsParams(this RftsParamsLevelModel level)
        {
            return new RftsParamsLevel
            {
                LevelName = level.Code, 
                Enabled = level.IsEnabled,
                LevelThresholdSet = new RftsLevelThresholdSet()
                {
                    Lt = level.Lines.First(l => l.Code == @"Lt").ToRftsParams(),
                    Rt = level.Lines.First(l => l.Code == @"Rt").ToRftsParams(),
                    Ct = level.Lines.First(l => l.Code == @"Ct").ToRftsParams()
                },
                Eelt = level.Lines.First(l => l.Code == @"Eelt").ToRftsParams(),
            };
        }

        private static Threshold ToRftsParams(this ThresholdLine line)
        {
            var threshold = new Threshold() { Absolute = !line.IsRelative };
            if (line.IsRelative)
                threshold.RelativeThreshold = (int)(line.Value * 1000);
            else
                threshold.AbsoluteThreshold = (int)(line.Value * 1000);
            return threshold;
        }

        private static RftsUniParameter ToRftsParams(this RftsUniParamModel uniParamModel)
        {
            return new RftsUniParameter()
            {
                Name = uniParamModel.Code,
                Value = (int)(uniParamModel.Value * uniParamModel.Scale),
                Scale = uniParamModel.Scale,
            };
        }

    }
}