using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;
using Optixsoft.SorExaminer.OtdrDataFormat;

namespace Iit.Fibertest.Client
{
    public static class BaseRefMeasParamsChecker
    {
        public static string IsBaseRefHasAcceptableMeasParams(OtdrDataKnownBlocks otdrKnownBlocks, SetOfAcceptableMeasParams acceptableMeasParams)
        {
            var units = ParseLineOfVariantsForParam(acceptableMeasParams.Units);
            if (units == null)
                return @"There are no acceptable measurement parameters for RTU!";
            var waveLength = otdrKnownBlocks.GeneralParameters.NominalWavelength.ToString(CultureInfo.InvariantCulture);
            if (!units.Contains($@"SM{waveLength}"))
                return string.Format(Resources.SID_Invalid_parameter___Wave_length__0_, waveLength);

            //TODO check other parameters
            return "";
        }

        private static List<string> ParseLineOfVariantsForParam(string value)
        {
            if (value == null) return null;
            // if there is only one variant it will be returned without leading slash
            if (value[0] != '/')
                return new List<string> { value };

            var strs = value.Split('/');
            return strs.Skip(1).ToList();
        }

    }
}