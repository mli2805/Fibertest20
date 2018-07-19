using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.SuperClient
{
    public class AddServerViewModel : Screen
    {
        private readonly FtServerList _ftServerList;

        public string ServerTitle { get; set; }

        public string ServerIp { get; set; }

        public int ServerTcpPort { get; set; } = 11840;

        public string Username { get; set; } = "superclient";

        public string Password { get; set; } = "superclient";

        public AddServerViewModel(FtServerList ftServerList)
        {
            _ftServerList = ftServerList;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Add_server;
            ServerTitle = "";
            ServerIp = "";
        }
        public void CheckConnection()
        {

        }
        public void Save()
        {
            var maxPostfix = _ftServerList.Servers.Any() ? _ftServerList.Servers.Select(x => x.Entity).Max(e => e.Postfix) : 0;

            var ftServerEntity = new FtServerEntity()
            {
                Postfix = maxPostfix + 1,
                ServerTitle = ServerTitle,
                ServerIp = ServerIp,
                ServerTcpPort = ServerTcpPort,
                Username = Username,
                Password = Password,
            };
            _ftServerList.Add(new FtServer(){Entity = ftServerEntity});
            TryClose();
        }

        public void Cancel()
        {
            TryClose();
        }

    }
}
