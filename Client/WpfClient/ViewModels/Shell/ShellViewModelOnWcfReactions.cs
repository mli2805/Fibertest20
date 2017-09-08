using System;

namespace Iit.Fibertest.Client
{
    public partial class ShellViewModel
    {
        private void ClientWcfService_MessageReceived(object e)
        {
            _logFile.AppendLine(@"something received by wcf");
        }
    }
}
