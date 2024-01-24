using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Iit.Fibertest.SuperClient
{
    public class ChildStarter
    {
        private readonly GasketViewModel _gasketViewModel;
        private readonly Dictionary<int, Process> _processes = new Dictionary<int, Process>();

        public ChildStarter(GasketViewModel gasketViewModel)
        {
            _gasketViewModel = gasketViewModel;
        }

        public void StartFtClient(FtServerEntity ftServerEntity)
        {
            var process = StartChild(ftServerEntity);
            _processes.Add(ftServerEntity.Postfix, process);
        }

        public void PlaceFtClientOnPanel(int postfix)
        {
            if (_processes.TryGetValue(postfix, out var process))
                _gasketViewModel.PutProcessOnPanel(process, postfix);
        }

        public void CleanOnLoadingFailed(int postfix)
        {
            _processes.Remove(postfix);
        }

        public void CleanAfterClosing(FtServerEntity ftServerEntity)
        {
            _processes.Remove(ftServerEntity.Postfix);
            _gasketViewModel.RemoveTabItem(ftServerEntity.Postfix);
        }

        private Process StartChild(FtServerEntity ftServerEntity)
        {
            var clientFilename = ftServerEntity.ClientFolder + @"\bin\Iit.Fibertest.Client.exe";

            var process = new Process
            {
                StartInfo = {
                    FileName = clientFilename,
                    Arguments = $@"{ftServerEntity.Postfix} {Thread.CurrentThread.CurrentUICulture} {
                        ftServerEntity.Username} {ftServerEntity.Password} {Guid.NewGuid()} {ftServerEntity.ServerIp
                        } {ftServerEntity.ServerTcpPort} " + "\""  + ftServerEntity.ServerTitle + "\"",
                }
            };
            process.Start();
            Thread.Sleep(TimeSpan.FromSeconds(1));
            return process;
        }
    }
}
