using Dto;
using Iit.Fibertest.Utils35;

namespace Client_WcfService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "ClientWcfService" in both code and config file together.
    public class ClientWcfService : IClientWcfService
    {
        public static IniFile ClientIni { get; set; }
        public static Logger35 ClientLog { get; set; }
        public void ConfirmRtuInitialized(RtuInitialized rtu)
        {
            ClientLog.AppendLine($"{rtu.Serial}");
        }
    }
}
