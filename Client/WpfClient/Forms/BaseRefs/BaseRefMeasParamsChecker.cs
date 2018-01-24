using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;
using Optixsoft.SorExaminer.OtdrDataFormat;

namespace Iit.Fibertest.Client
{
    public class BaseRefMeasParamsChecker
    {
        private readonly IWindowManager _windowManager;

        public BaseRefMeasParamsChecker(IWindowManager windowManager)
        {
            _windowManager = windowManager;
        }

        public bool IsBaseRefHasAcceptableMeasParams(OtdrDataKnownBlocks otdrKnownBlocks, 
            TreeOfAcceptableMeasParams acceptableMeasParams, string errorHeader)
        {
            var units = acceptableMeasParams.Units.Keys.ToList();
            var waveLength = otdrKnownBlocks.GeneralParameters.NominalWavelength.ToString(CultureInfo.InvariantCulture);
            if (!units.Contains($@"SM{waveLength}"))
            {
                var vm = new MyMessageBoxViewModel(MessageType.Error, new List<string>()
                {
                    errorHeader,
                    "",
                    "",
                    Resources.SID_Measurement_parameters_are_not_compatible_with_this_RTU,
                    "",
                    string.Format(Resources.SID_Invalid_parameter___Wave_length__0_, waveLength),
                }, 5);
                _windowManager.ShowDialogWithAssignedOwner(vm);
                return false;
            }

            //TODO check other parameters
            return true;
        }

    }
}