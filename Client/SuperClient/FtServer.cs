using Caliburn.Micro;

namespace Iit.Fibertest.SuperClient
{
    public enum FtServerState
    {
        Disconnected,
        Connected,
        Breakdown,
    }

    public class FtServerEntity
    {
        public int Id { get; set; }
        public int Postfix { get; set; } // used for ini and log file names
        public string ServerTitle { get; set; }
        public string ServerIp { get; set; }
        public int ServerTcpPort { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
    public class FtServer : PropertyChangedBase
    {
        public FtServerEntity Entity { get; set; }

        public string ServerName => $"{Entity.ServerTitle} ({Entity.ServerIp})";

        private FtServerState _serverConnectionState;
        public FtServerState ServerConnectionState
        {
            get { return _serverConnectionState; }
            set
            {
                if (value == _serverConnectionState) return;
                _serverConnectionState = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(ConnectionStatePictogram));
                NotifyOfPropertyChange(nameof(IsConnectEnabled));
                NotifyOfPropertyChange(nameof(IsDisconnectEnabled));
            }
        }

        private FtServerState _tracesState;

        public FtServerState TracesState
        {
            get { return _tracesState; }
            set
            {
                if (value == _tracesState) return;
                _tracesState = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(TracesStatePictogram));
            }
        }

        public bool IsConnectEnabled => ServerConnectionState != FtServerState.Connected;
        public bool IsDisconnectEnabled => ServerConnectionState == FtServerState.Connected;
       

        public string ConnectionStatePictogram => GetPathToPictogram(ServerConnectionState);
        public string TracesStatePictogram => GetPathToPictogram(TracesState);

        private string GetPathToPictogram(FtServerState state)
        {
            switch (state)
            {
                case FtServerState.Disconnected:
                    return @"pack://application:,,,/Resources/EmptySquare.png";
                case FtServerState.Connected:
                    return @"pack://application:,,,/Resources/GreenSquare.png";
                case FtServerState.Breakdown:
                    return @"pack://application:,,,/Resources/RedSquare.png";
                default:
                    return @"pack://application:,,,/Resources/EmptySquare.png";
            }
        }
    }
}