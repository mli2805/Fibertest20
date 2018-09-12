using Caliburn.Micro;

namespace Iit.Fibertest.SuperClient
{
    public class FtServer : PropertyChangedBase
    {
        public FtServerEntity Entity { get; set; }

        public string ServerName => $@"{Entity.ServerTitle} ({Entity.ServerIp})";

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

        private FtSystemState _systemState;

        public FtSystemState SystemState
        {
            get => _systemState;
            set
            {
                if (value == _systemState) return;
                _systemState = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(SystemStatePictogram));
            }
        }

        public bool IsConnectEnabled => ServerConnectionState != FtServerConnectionState.Connected;
        public bool IsDisconnectEnabled => ServerConnectionState == FtServerConnectionState.Connected;


        public string ConnectionStatePictogram => GetPathToConnectionPictogram(ServerConnectionState);
        public string SystemStatePictogram => GetPathToSystemStatePictogram(SystemState);

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

        private string GetPathToSystemStatePictogram(FtSystemState state)
        {
            switch (state)
            {
                case FtSystemState.Unknown:
                    return @"pack://application:,,,/Resources/EmptySquare.png";
                case FtSystemState.Ok:
                    return @"pack://application:,,,/Resources/GreenSquare.png";
                case FtSystemState.Failed:
                    return @"pack://application:,,,/Resources/RedSquare.png";
                default:
                    return @"pack://application:,,,/Resources/EmptySquare.png";
            }
        }
    }
}