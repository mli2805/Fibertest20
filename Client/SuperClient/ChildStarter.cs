using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Iit.Fibertest.UtilsLib;
// ReSharper disable LocalizableElement

namespace Iit.Fibertest.SuperClient
{
    public class ChildStarter
    {
        private readonly GasketViewModel _gasketViewModel;
        private readonly Dictionary<int, Process> _processes = new Dictionary<int, Process>();

        private const string DebugClientFilename = @"c:\VsGitProjects\Fibertest20\Client\WpfClient\bin\Debug\Iit.Fibertest.Client.exe";
        private const string ReleaseClientFilename = @"c:\Iit-Fibertest\Client\bin\Iit.Fibertest.Client.exe";
        private readonly string _clientFilename;

        public ChildStarter(IniFile iniFile, GasketViewModel gasketViewModel)
        {
            _gasketViewModel = gasketViewModel;
            _clientFilename = iniFile.Read(IniSection.Miscellaneous, IniKey.PathToClient, ReleaseClientFilename);
#if DEBUG
            _clientFilename = DebugClientFilename;
#endif
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
            var process = new Process
            {
                StartInfo = {
                    FileName = _clientFilename,
                    Arguments = $@"{ftServerEntity.Postfix} {Thread.CurrentThread.CurrentUICulture} {ftServerEntity.Username} {ftServerEntity.Password} {Guid.NewGuid()} {ftServerEntity.ServerIp} {ftServerEntity.ServerTcpPort}" 
                                + "\""  + ftServerEntity.ServerTitle + "\"",
                }
            };
            process.Start();
            Thread.Sleep(TimeSpan.FromSeconds(1));
            return process;
        }
    }
}
