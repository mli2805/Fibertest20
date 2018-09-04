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

        public async Task<int> ClientLoadingResult(int postfix, bool isLoadedOk, bool isStateOk)
        {
            var wcfConnection = _wcfFactory.GetC2SChannelFactory();
            if (wcfConnection == null)
                return -1;

            try
            {
                var channel = wcfConnection.CreateChannel();
                var result = await channel.ClientLoadingResult(postfix, isLoadedOk, isStateOk);
                wcfConnection.Close();
                return result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                return -1;
            }
        }

        public async Task<int> NotifyConnectionBroken(int postfix)
        {
            var wcfConnection = _wcfFactory.GetC2SChannelFactory();
            if (wcfConnection == null)
                return -1;

            try
            {
                var channel = wcfConnection.CreateChannel();
                var result = await channel.NotifyConnectionBroken(postfix);
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

        public async Task<int> SetSystemState(int postfix, bool isStateOk)
        {
            var wcfConnection = _wcfFactory.GetC2SChannelFactory();
            if (wcfConnection == null)
                return -1;

            try
            {
                var channel = wcfConnection.CreateChannel();
                var result = await channel.SetSystemState(postfix, isStateOk);
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