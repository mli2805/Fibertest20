using System.Collections.Generic;
using Caliburn.Micro;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.SuperClient
{
    public class ClientVersionsViewModel : Screen
    {
        public List<string> Lines { get; set; }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Client_software_versions;
        }

        public void Initialize(List<string> lines)
        {
            Lines = lines;
        }
    }
}
