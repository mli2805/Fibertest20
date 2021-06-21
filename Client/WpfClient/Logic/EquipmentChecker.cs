using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WpfCommonViews;

namespace Iit.Fibertest.Client
{
    public static class EquipmentChecker
    {

        public static bool EquipmentCanBeChanged(this Model readModel, Guid equipmentId, IWindowManager windowManager)
        {
            foreach (var trace in readModel.Traces)
            {
                if (trace.EquipmentIds.Contains(equipmentId) && trace.IsIncludedInMonitoringCycle)
                {
                    var rtu = readModel.Rtus.FirstOrDefault(r => r.Id == trace.RtuId);
                    if (rtu != null && rtu.MonitoringState == MonitoringState.On)
                    {
                        windowManager.ShowDialogWithAssignedOwner(
                            new MyMessageBoxViewModel(MessageType.Error, new List<string>()
                            {
                                Resources.SID_This_equipment_could_not_be_changed_,
                                "",
                                Resources.SID_There_are_traces_which_use_this_equipment_and_are_under_monitoring_now_,
                                Resources.SID_Stop_monitoring_in_order_to_change_equipment
                            }, 0));
                         
                        return false;
                    }
                }
            }
            return true;
        }
    }
}