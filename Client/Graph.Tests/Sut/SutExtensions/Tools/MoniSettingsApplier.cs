using System.Linq;
using Iit.Fibertest.Client;
using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;

namespace Graph.Tests
{
    public static class MoniSettingsApplier
    {
        public static void ApplyMoniSettings(this SystemUnderTest sut, RtuLeaf rtuLeaf, MonitoringState monitoringState)
        {
            sut.FakeWindowManager.RegisterHandler(model => MoniSettingsApplyHandler(model, monitoringState));
            rtuLeaf.MyContextMenu.First(i=>i?.Header == Resources.SID_Monitoring_settings).Command.Execute(rtuLeaf);
            sut.Poller.EventSourcingTick().Wait();
        }

        private static bool MoniSettingsApplyHandler(object model, MonitoringState monitoringState)
        {
            if (!(model is MonitoringSettingsViewModel vm)) return false;

            vm.Model.Charons[0].GroupenCheck = true;
            if (vm.Model.Charons.Count > 1)
                vm.Model.Charons[1].GroupenCheck = true;
            vm.Model.IsMonitoringOn = monitoringState == MonitoringState.On;
            vm.Apply().Wait();
            return true;
        }
    }
}
