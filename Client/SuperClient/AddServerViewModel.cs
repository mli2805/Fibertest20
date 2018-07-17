using Caliburn.Micro;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.SuperClient
{
    public class AddServerViewModel : Screen
    {
        private readonly FtServerList _ftServerList;

        public string ServerTitle { get; set; } = "fff";

        public string ServerIp { get; set; } = "fff";

        public int ServerTcpPort { get; set; } = 11840;

        public string Username { get; set; } = "fff";

        public string Password { get; set; } = "fff";

        public AddServerViewModel(FtServerList ftServerList)
        {
            _ftServerList = ftServerList;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Add_server;
        }
        public void CheckConnection()
        {

        }
        public void Save()
        {
            var ftServer = new FtServer()
            {
                ServerTitle = ServerTitle,
                ServerIp = ServerIp,
                ServerTcpPort = ServerTcpPort,
                Username = Username,
                Password = Password,
            };
         //   _ftServerList.Add(ftServer);
            TryClose();
        }

        public void Cancel()
        {
            TryClose();
        }

    }
}
