using System;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.SuperClientWcfServiceInterface;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.WcfConnections
{
    public class C2SWcfManager : IWcfServiceInSuperClient
    {
        private readonly IMyLog _logFile;
        private WcfFactory _wcfFactory;

        public C2SWcfManager(IniFile iniFile, IMyLog logFile)
        {
            _logFile = logFile;
            _wcfFactory = new WcfFactory(new DoubleAddress(){Main = new NetAddress("localhost", 11839)}, iniFile, _logFile);
        }

        public async Task<int> ClientLoaded(int postfix, bool isStateOk)
        {
            var wcfConnection = _wcfFactory.GetC2SChannelFactory();
            if (wcfConnection == null)
                return -1;

            try
            {
                var channel = wcfConnection.CreateChannel();
                var result = await channel.ClientLoaded(postfix, isStateOk);
                wcfConnection.Close();
                return result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                return -1;
            }
        }

        public async Task<int> ClientClosed(int postfix)
        {
            var wcfConnection = _wcfFactory.GetC2SChannelFactory();
            if (wcfConnection == null)
                return -1;

            try
            {
                var channel = wcfConnection.CreateChannel();
                var result = await channel.ClientClosed(postfix);
                wcfConnection.Close();
                return result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                return -1;
            }
        }

        public async Task<int> ClientStateChanged(int postfix)
        {
            var wcfConnection = _wcfFactory.GetC2SChannelFactory();
            if (wcfConnection == null)
                return -1;

            try
            {
                var channel = wcfConnection.CreateChannel();
                var result = await channel.ClientStateChanged(postfix);
                wcfConnection.Close();
                return result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                return -1;
            }
        }
    }
}