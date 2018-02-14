using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Optixsoft.SorExaminer.OtdrDataFormat;

namespace Iit.Fibertest.Client
{
    public class BaseRefLandmarksChecker
    {
        private readonly ReadModel _readModel;
        private readonly IWindowManager _windowManager;

        public BaseRefLandmarksChecker(ReadModel readModel, IWindowManager windowManager)
        {
            _readModel = readModel;
            _windowManager = windowManager;
        }

        // not only checks but adjust empty nodes if needed
        public bool IsBaseRefLandmarksCountMatched(OtdrDataKnownBlocks otdrKnownBlocks, Trace trace, string baseName)
        {
            var landmarkCount = otdrKnownBlocks.LinkParameters.LandmarksCount;

            var equipments = _readModel.GetTraceEquipments(trace).ToList(); // without RTU
            var nodesCount = equipments.Count(eq => eq.Type > EquipmentType.AdjustmentPoint) + 1; // without adjustment points
            var equipmentsCount = equipments.Count(eq => eq.Type > EquipmentType.CableReserve) + 1; // sic! in this case CableReserve is not an equipment

            if (landmarkCount == nodesCount || landmarkCount == equipmentsCount) return true;

            string errorStrings = BuildErrorStrings(landmarkCount, nodesCount, equipmentsCount);
            ShowError(errorStrings, trace, baseName);
            return false;
        }

        private string BuildErrorStrings(int landmarksCount, int nodesCount, int equipmentsCount)
        {
            return  string.Format(Resources.SID_Landmarks_count_in_reflectogram_is__0_, landmarksCount) + Environment.NewLine +
                Environment.NewLine +
                string.Format(Resources.SID_Trace_s_node_count_is__0_, nodesCount) + Environment.NewLine +
                string.Format(Resources.SID_Trace_s_equipment_count_is__0_, equipmentsCount);
        }

        private void ShowError(string errorStrings, Trace trace, string baseName)
        {
            var messageStrings = new List<string>() {
                string.Format(Resources.SID__0__base_is_not_compatible_with_trace, baseName),
                trace.Title, "", ""
            };
            messageStrings.Add(errorStrings);
            var vm = new MyMessageBoxViewModel(MessageType.Error, messageStrings, 4);
            _windowManager.ShowDialogWithAssignedOwner(vm);
        }
    }
}