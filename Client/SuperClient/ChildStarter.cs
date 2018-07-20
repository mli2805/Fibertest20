using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Iit.Fibertest.SuperClient
{
    public class ChildStarter
    {
        private readonly GasketViewModel _gasketViewModel;
        private Dictionary<int, Process> _processes = new Dictionary<int, Process>();

        const string ClientFilename = @"c:\VsGitProjects\SuperClientE\LittleClient\bin\Debug\LittleClient";
        //                const string ClientFilename = @"c:\VsGitProjects\Fibertest\Client\WpfClient\bin\Debug\Iit.Fibertest.Client.exe";

        public ChildStarter(GasketViewModel gasketViewModel)
        {
            _gasketViewModel = gasketViewModel;
        }

        public void StartFtClient(FtServerEntity ftServerEntity)
        {
            var process = StartChild(ftServerEntity);
            _processes.Add(ftServerEntity.Postfix, process);

            _gasketViewModel.PutProcessOnPanel(process, ftServerEntity.Postfix);
        }

        public void CloseFtClient(FtServerEntity ftServerEntity)
        {
            var process = _processes[ftServerEntity.Postfix];
            process.Kill();
            _processes.Remove(ftServerEntity.Postfix);
            _gasketViewModel.RemoveTabItem(ftServerEntity.Postfix);
        }

        private Process StartChild(FtServerEntity ftServerEntity)
        {
            var process = new Process
            {
                StartInfo = {
                    FileName = ClientFilename,
                    Arguments = $"{ftServerEntity.Postfix} {ftServerEntity.Username} {ftServerEntity.Password} {ftServerEntity.ServerIp} {ftServerEntity.ServerTcpPort}"
                }
            };
            process.Start();
            //                        var pause = ftServerEntity.Postfix == 4 ? 45 : 10;
            var pause = 2;
            Thread.Sleep(TimeSpan.FromSeconds(pause));
            return process;
        }
    }
}
