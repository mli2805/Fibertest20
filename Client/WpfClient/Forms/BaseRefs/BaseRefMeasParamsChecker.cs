using System.Globalization;
using System.Linq;
using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;
using Optixsoft.SorExaminer.OtdrDataFormat;

namespace Iit.Fibertest.Client
{
    public static class BaseRefMeasParamsChecker
    {
        public static string IsBaseRefHasAcceptableMeasParams(OtdrDataKnownBlocks otdrKnownBlocks, TreeOfAcceptableMeasParams acceptableMeasParams)
        {
            var units = acceptableMeasParams.Units.Keys.ToList();
            var waveLength = otdrKnownBlocks.GeneralParameters.NominalWavelength.ToString(CultureInfo.InvariantCulture);
            if (!units.Contains($@"SM{waveLength}"))
                return string.Format(Resources.SID_Invalid_parameter___Wave_length__0_, waveLength);

            //TODO check other parameters
            return "";
        }

    }
}