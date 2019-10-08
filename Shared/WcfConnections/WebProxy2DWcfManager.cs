using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfServiceForWebProxyInterface;

namespace Iit.Fibertest.WcfConnections
{
    public class WebProxy2DWcfManager : IWcfServiceForWebProxy
    {
        private readonly IniFile _iniFile;
        private readonly IMyLog _logFile;
        private WcfFactory _wcfFactory;

        public WebProxy2DWcfManager(IniFile iniFile, IMyLog logFile)
        {
            _iniFile = iniFile;
            _logFile = logFile;
        }
        public void SetServerAddresses(DoubleAddress newServerAddress, string username, string clientIp)
        {
            _wcfFactory = new WcfFactory(newServerAddress, _iniFile, _logFile);
        }
        public async Task<List<RtuDto>> GetRtuList()
        {
            var wcfConnection = _wcfFactory.GetWebProxy2DChannelFactory();
            if (wcfConnection == null)
                return null;

            try
            {
                var channel = wcfConnection.CreateChannel();
                var result = await channel.GetRtuList();
                wcfConnection.Close();
                return result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("GetRtuList: " + e.Message);
                return null;
            }
        }

        public async Task<List<OpticalEventDto>> GetOpticalEventList()
        {
            var wcfConnection = _wcfFactory.GetWebProxy2DChannelFactory();
            if (wcfConnection == null)
                return null;

            try
            {
                var channel = wcfConnection.CreateChannel();
                var result = await channel.GetOpticalEventList();
                wcfConnection.Close();
                return result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("GetOpticalEventList: " + e.Message);
                return null;
            }
        }
    }
}