using Caliburn.Micro;

namespace Iit.Fibertest.WpfCommonViews
{
    public class WaitViewModel : Screen
    {
        protected override void OnViewLoaded(object view)
        {
            DisplayName = StringResources.Resources.SID_Long_operation__please_wait;
        }
    }
}
