using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.IitOtdrLibrary;
using Iit.Fibertest.StringResources;
using Optixsoft.SorExaminer.OtdrDataFormat;

namespace Iit.Fibertest.Client
{
    public class BaseRefsChecker
    {
        private readonly ReadModel _readModel;
        private readonly IWindowManager _windowManager;

        public BaseRefsChecker(ReadModel readModel, IWindowManager windowManager)
        {
            _readModel = readModel;
            _windowManager = windowManager;
        }

        public bool IsBaseRefsAcceptable(List<BaseRefDto> baseRefsDto, Trace trace)
        {
            var rtu = _readModel.Rtus.FirstOrDefault(r => r.Id == trace.RtuId);
            if (rtu == null)
                return false;

            foreach (var baseRefDto in baseRefsDto)
            {
                var res = SorData.TryGetFromBytes(baseRefDto.SorBytes, out var otdrKnownBlocks);
                if (res != "")
                {
                    var vm = new NotificationViewModel(Resources.SID_Error_, 
                        string.Format(Resources.SID_File_read_error__0__1_, Environment.NewLine, res));
                    _windowManager.ShowDialog(vm);
                    return false;
                }

                var message = IsBaseRefHasAcceptableMeasParams(otdrKnownBlocks, rtu.AcceptableMeasParams);
                if (message != "")
                {
                    var vm = new NotificationViewModel(Resources.SID_Error_, 
                        string.Format(Resources.SID_Invalid_measuring_parameters__0__Not_compatible_with_this_RTU_, Environment.NewLine, message));
                    _windowManager.ShowDialog(vm);
                    return false;
                }
            }
            return true;
        }

        private string IsBaseRefHasAcceptableMeasParams(OtdrDataKnownBlocks otdrKnownBlocks, SetOfAcceptableMeasParams acceptableMeasParams)
        {
            var units = ParseLineOfVariantsForParam(acceptableMeasParams.Units);
            var waveLength = otdrKnownBlocks.FixedParameters.ActualWavelength.ToString(CultureInfo.InvariantCulture);
            if (!units.Contains($@"SM{waveLength}"))
                return string.Format(Resources.SID_Invalid_parameter___Wave_length__0_, waveLength);

            //TODO check other parameters
            return "";
        }

        public List<string> ParseLineOfVariantsForParam(string value)
        {
            // if there is only one variant it will be returned without leading slash
            if (value[0] != '/')
                return new List<string> { value };

            var strs = value.Split('/');
            return strs.Skip(1).ToList();
        }
    }
}
