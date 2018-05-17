using Caliburn.Micro;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class AboutViewModel : Screen
    {
        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_About;
        }
    }
}
