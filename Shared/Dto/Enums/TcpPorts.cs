namespace Iit.Fibertest.Dto
{
    public enum TcpPorts
    {
        // DataCenterService
        ServerListenToClient = 11840,
        ServerListenToRtu = 11841,
        ServerListenToWebProxy = 11838,
        ServerListenToC2R = 11837,

        // DataCenterWebApi
        WebProxyListenTo = 11080,

        // RTU
        RtuListenTo = 11842,
        RtuVeexListenTo = 80,

        // Client
        ClientListenTo = 11843, // when started under SuperClient: 11843 + _commandLineParameters.ClientOrdinal

        // SuperClient
        SuperClientListenTo = 11839,
    }
}
