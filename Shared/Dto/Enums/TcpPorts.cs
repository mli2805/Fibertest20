namespace Iit.Fibertest.Dto
{
    public enum TcpPorts
    {
        // DataCenterService
        ServerListenToDesktopClient = 11840,
        ServerListenToWebClient = 11838,
        ServerListenToCommonClient = 11837,

        ServerListenToRtu = 11841,

        // DataCenterWebApi
        WebApiListenTo = 11080,

        // RTU
        RtuListenTo = 11842,
        RtuVeexListenTo = 80,
        RtuListenToGrpc = 11942,
        RtuListenToHttp = 11980,

        // Client
        ClientListenTo = 11843, // when started under SuperClient: 11843 + _commandLineParameters.ClientOrdinal

        // SuperClient
        SuperClientListenTo = 11839,

        // БОП - IIT additional OTAU block
        IitBop = 11834,
    }
}
