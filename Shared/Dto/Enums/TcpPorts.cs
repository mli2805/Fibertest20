namespace Iit.Fibertest.Dto
{
    public enum TcpPorts
    {
        ServerListenToClient = 11840,
        ServerListenToRtu = 11841,
        ServerListenToWebProxy = 11838,
        WebProxyListenTo = 11837,
        RtuListenTo = 11842,
        RtuVeexListenTo = 80,
        ClientListenTo = 11843, // when started under SuperClient: 11843 + _commandLineParameters.ClientOrdinal

        SuperClientListenTo = 11839,
    }
}
