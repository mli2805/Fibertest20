using System;
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
            Clients = clientList.Clients.Select(c=>new ClientRadioButton(c)).ToList();
            Clients.First().IsChecked = true;

            Header = new List<string>
            {
                Resources.SID_Connection_established_successfully_,
                "",
                $"Software Data Center version is {serverVersion}",
                "Client software of this version was not found, ",
                "but the following versions are installed on the computer:",
                ""
            };
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Select Client software";
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
