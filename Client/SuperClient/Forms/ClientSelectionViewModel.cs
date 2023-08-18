using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.SuperClient
{
    public class ClientRadioButton
    {
        public bool IsChecked { get; set; }
       
        public string Version;
        public string Path;

        public string RadioButtonContent => $@"{Version} ({Path})";

        public ClientRadioButton(ClientForSuper client)
        {
            Version = client.Version;
            Path = client.Path;
        }
    }
    public class ClientSelectionViewModel : Screen
    {
        public bool IsApplyPressed { get; set; }

        public List<string> Header { get; set; }
        public List<ClientRadioButton> Clients { get; set; }

        public void Initialize(string serverVersion, ClientList clientList)
        {
            Clients = clientList.Clients.Where(l=>l.Version != null).Select(c=>new ClientRadioButton(c)).ToList();
            Clients.First().IsChecked = true;

            Header = new List<string>
            {
                Resources.SID_Remote_Server_software_version_is_ + serverVersion,
                "",
                Resources.SID_Versions_of_Client_software_installed_at_the_workplace_,
                ""
            };
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Select_client_software_version;
        }

        public void Apply()
        {
            IsApplyPressed = true;
            TryClose();
        }

        public void Cancel()
        {
            TryClose();
        }
    }
}
