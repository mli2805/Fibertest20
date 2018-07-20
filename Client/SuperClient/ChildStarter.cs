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

        //        const string ClientFilename = @"c:\VsGitProjects\SuperClientE\LittleClient\bin\Debug\LittleClient";
        const string ClientFilename = @"c:\VsGitProjects\Fibertest\Client\WpfClient\bin\Debug\Iit.Fibertest.Client.exe";

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
            if (_processes.ContainsKey(postfix))
                _gasketViewModel.PutProcessOnPanel(_processes[postfix], postfix);
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
                    Arguments = $"{ftServerEntity.Postfix} {Thread.CurrentThread.CurrentUICulture} {ftServerEntity.Username} {ftServerEntity.Password} {ftServerEntity.ServerIp} {ftServerEntity.ServerTcpPort}"
                }
            };
            process.Start();
            Thread.Sleep(TimeSpan.FromSeconds(1));
            return process;
        }
    }
}
