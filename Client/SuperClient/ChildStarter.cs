using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Forms.Integration;
using Panel = System.Windows.Forms.Panel;

namespace Iit.Fibertest.SuperClient
{
    public class ChildStarter
    {
        #region interop
        private const int GwlStyle = -16;
        private const int WsBorder = 0x00800000;
        private const int WsChild = 0x40000000;
        private const int SwShowmaximized = 3;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        private static extern IntPtr SetParent(HandleRef hWnd, HandleRef hWndParent);

        [DllImport("user32.dll")]
        private static extern bool ShowWindowAsync(HandleRef hWnd, int nCmdShow);
        #endregion

        const string ClientFilename = @"c:\VsGitProjects\SuperClientE\LittleClient\bin\Debug\LittleClient";
//                const string ClientFilename = @"c:\VsGitProjects\Fibertest\Client\WpfClient\bin\Debug\Iit.Fibertest.Client.exe";

      
        public Panel CreatePanel(TabItem tabItem)
        {
            var windowsFormsHost = new WindowsFormsHost();
            tabItem.Content = windowsFormsHost;

            Panel panel = new Panel();
            windowsFormsHost.Child = panel;

            return panel;
        }


        public void PutChildOnPanel(Process childProcess, Panel panel)
        {
            IntPtr childHandle = childProcess.MainWindowHandle;
            int oldStyle = GetWindowLong(childHandle, GwlStyle);
            SetWindowLong(childHandle, GwlStyle, (oldStyle | WsChild) & ~WsBorder);
            var parentHandle = new HandleRef(null, panel.Handle);
            SetParent(new HandleRef(null, childHandle), parentHandle);
            var childHandleRef = new HandleRef(null, childHandle);
            ShowWindowAsync(childHandleRef, SwShowmaximized);
        }

        public Process StartChild(FtServerEntity ftServerEntity)
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
