using Dto;

namespace Iit.Fibertest.Client
{
    public partial class ShellViewModel
    {
        private void ClientWcfService_MessageReceived(object e)
        {
            var dto = e as MonitoringResultDto;
            if (dto != null)
                _logFile.AppendLine(@"Moniresult happened");
        }
    }
}
