using System.Collections.Generic;
using Caliburn.Micro;

namespace Iit.Fibertest.SuperClient
{
    public class ClientVersionsViewModel : Screen
    {
        public List<string> Lines { get; set; }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Client software versions";
        }

        public void Initialize(List<string> lines)
        {
            Lines = lines;
        }
    }
}
