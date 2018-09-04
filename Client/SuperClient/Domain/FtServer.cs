using Caliburn.Micro;

namespace Iit.Fibertest.SuperClient
{
    public class FtServer : PropertyChangedBase
    {
        public FtServerEntity Entity { get; set; }

        public string ServerName => $"{Entity.ServerTitle} ({Entity.ServerIp})";

        private FtServerConnectionState _serverConnectionState;
        public FtServerConnectionState ServerConnectionState
        {
            get => _serverConnectionState;
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

        private FtServerState _serverState;

        public FtServerState ServerState
        {
            get => _serverState;
            set
            {
                if (value == _serverState) return;
                _serverState = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(ServerStatePictogram));
            }
        }

        public bool IsConnectEnabled => ServerConnectionState != FtServerConnectionState.Connected;
        public bool IsDisconnectEnabled => ServerConnectionState == FtServerConnectionState.Connected;


        public string ConnectionStatePictogram => GetPathToConnectionPictogram(ServerConnectionState);
        public string ServerStatePictogram => GetPathToStatePictogram(ServerState);

        private string GetPathToConnectionPictogram(FtServerConnectionState state)
        {
            switch (state)
            {
                case FtServerConnectionState.Disconnected:
                    return @"pack://application:,,,/Resources/EmptySquare.png";
                case FtServerConnectionState.Connected:
                    return @"pack://application:,,,/Resources/GreenSquare.png";
                case FtServerConnectionState.Breakdown:
                    return @"pack://application:,,,/Resources/RedSquare.png";
                default:
                    return @"pack://application:,,,/Resources/EmptySquare.png";
            }
        }

        private string GetPathToStatePictogram(FtServerState state)
        {
            switch (state)
            {
                case FtServerState.Unknown:
                    return @"pack://application:,,,/Resources/EmptySquare.png";
                case FtServerState.Ok:
                    return @"pack://application:,,,/Resources/GreenSquare.png";
                case FtServerState.Failed:
                    return @"pack://application:,,,/Resources/RedSquare.png";
                default:
                    return @"pack://application:,,,/Resources/EmptySquare.png";
            }
        }
    }
}