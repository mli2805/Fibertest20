using System.Collections.Generic;
using System.Globalization;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WpfCommonViews;

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
            if (!IsWaveLengthAcceptable(otdrKnownBlocks, acceptableMeasParams, errorHeader))
                return false;
            //TODO check other parameters
            return true;
        }

        private bool IsWaveLengthAcceptable(OtdrDataKnownBlocks otdrKnownBlocks,
            TreeOfAcceptableMeasParams acceptableMeasParams, string errorHeader)
        {
            var waveLength = otdrKnownBlocks.GeneralParameters.NominalWavelength.ToString(CultureInfo.InvariantCulture);
            foreach (var unit in acceptableMeasParams.Units.Keys) // "SM NNNN" or "SMNNNN"
            {
                if (unit.Contains(waveLength))
                    return true;
            }

            var vm = new MyMessageBoxViewModel(MessageType.Error, new List<string>()
            {
                errorHeader,
                "",
                "",
                Resources.SID_Measurement_parameters_are_not_compatible_with_this_RTU,
                "",
                string.Format(Resources.SID_Invalid_parameter___Wave_length__0_, waveLength),
            }, 5);
            _windowManager.ShowDialogWithAssignedOwner(vm); return false;
        }

    }
}