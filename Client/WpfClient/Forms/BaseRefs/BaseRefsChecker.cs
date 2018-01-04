using System;
using System.Collections.Generic;
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
        private readonly GraphGpsCalculator _graphGpsCalculator;

        public BaseRefsChecker(ReadModel readModel, IWindowManager windowManager, GraphGpsCalculator graphGpsCalculator)
        {
            _readModel = readModel;
            _windowManager = windowManager;
            _graphGpsCalculator = graphGpsCalculator;
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
                    _windowManager.ShowDialogWithAssignedOwner(vm);
                    return false;
                }

                var message = BaseRefMeasParamsChecker.IsBaseRefHasAcceptableMeasParams(otdrKnownBlocks, rtu.AcceptableMeasParams);
                if (message != "")
                {
                    var vm = new NotificationViewModel(Resources.SID_Error_,
                        string.Format(Resources.SID_Invalid_measuring_parameters__0__Not_compatible_with_this_RTU_, Environment.NewLine, message));
                    _windowManager.ShowDialogWithAssignedOwner(vm);
                    return false;
                }

                message = IsBaseRefCompatibleWithTrace(otdrKnownBlocks, trace);
                if (message != "")
                {
                    var vm = new NotificationViewModel(Resources.SID_Error_,
                        string.Format(Resources.SID__0__base_is_not_compatible_with_trace_1__2_, baseRefDto.BaseRefType.GetLocalizedFemaleString(), Environment.NewLine, message));
                    _windowManager.ShowDialogWithAssignedOwner(vm);
                    return false;
                }

                message = CompareDistances(otdrKnownBlocks, trace);
                var vm1 = new QuestionViewModel($"{message}{Environment.NewLine}Assign base reflectograms");
                _windowManager.ShowDialogWithAssignedOwner(vm1);
                if (!vm1.IsAnswerPositive)
                    return false;
            }
            return true;
        }

        private string IsBaseRefCompatibleWithTrace(OtdrDataKnownBlocks otdrKnownBlocks, Trace trace)
        {
            var keyEventsCount = otdrKnownBlocks.KeyEvents.KeyEvents.Length;
            var equipmentsCount = trace.Equipments.Count;
            if (keyEventsCount !=  equipmentsCount)
                return $@"{keyEventsCount} doesn't match {equipmentsCount}";
            return "";
        }

        private string CompareDistances(OtdrDataKnownBlocks otdrKnownBlocks, Trace trace)
        {
            var gpsDistance = _graphGpsCalculator.CalculateTraceGpsLength(trace);
            // TODO get trace length from OtdrDataKnownBlocks
            return $@"Trace length on map is {gpsDistance} km";
        }

     }
}
