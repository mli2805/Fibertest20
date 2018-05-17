using Caliburn.Micro;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class EventLogViewModel : Screen
    {
        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_User_operations_log;
        }
    }
}
