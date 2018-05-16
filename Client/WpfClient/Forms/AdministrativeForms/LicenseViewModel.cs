using Caliburn.Micro;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class LicenseViewModel : Screen
    {
        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_License;
        }
    }
}
