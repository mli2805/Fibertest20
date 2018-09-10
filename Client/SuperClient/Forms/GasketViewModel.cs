using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using Caliburn.Micro;
using System.Windows.Forms.Integration;
using System.Windows.Media;
using Iit.Fibertest.UtilsLib;
using Panel = System.Windows.Forms.Panel;

namespace Iit.Fibertest.SuperClient
{
    public class GasketViewModel : PropertyChangedBase
    {
        private readonly IMyLog _logFile;

        #region interop
        private const int GwlStyle = -16;
        private const int WsBorder = 0x00800000;
        private const int WsChild = 0x40000000;
        private const int SwShowMaximized = 3;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        private static extern IntPtr SetParent(HandleRef hWnd, HandleRef hWndParent);

        [DllImport("user32.dll")]
        private static extern bool ShowWindowAsync(HandleRef hWnd, int nCmdShow);
        #endregion

        public ObservableCollection<TabItem> Children { get; set; } = new ObservableCollection<TabItem>();

        private TabItem _selectedTabItem;
        public TabItem SelectedTabItem
        {
            get { return _selectedTabItem; }
            set
            {
                if (Equals(value, _selectedTabItem)) return;
                _selectedTabItem = value;
                NotifyOfPropertyChange();
            }
        }

        public GasketViewModel(IMyLog logFile)
        {
            _logFile = logFile;
        }

        public void PutProcessOnPanel(Process childProcess, int postfix)
        {
            var panel = CreatePanel(postfix);
            PutChildOnPanel(childProcess, panel);
        }

        private Panel CreatePanel(int postfix)
        {
            var tabItem = new TabItem() { Header = new ContentControl(), Tag = postfix, Background = Brushes.Bisque};
            Children.Add(tabItem);
            SelectedTabItem = tabItem;
            _logFile.AppendLine($"tabItem {tabItem.Height}  {tabItem.Width} {tabItem.ActualHeight} {tabItem.ActualWidth}");

            var windowsFormsHost = new WindowsFormsHost() { Background = Brushes.Aquamarine};
            tabItem.Content = windowsFormsHost;
            _logFile.AppendLine($"windowsFormsHost {windowsFormsHost.Height}  {windowsFormsHost.Width} {windowsFormsHost.ActualHeight} {windowsFormsHost.ActualWidth}");

            Panel panel = new Panel();
            var screenHeight = System.Windows.SystemParameters.PrimaryScreenHeight;
            var screenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;

            panel.Height = Convert.ToInt16(screenHeight) - 75;
            panel.Width = Convert.ToInt16(screenWidth) - 262;

            windowsFormsHost.Child = panel;
            _logFile.AppendLine($"panel {panel.Height}  {panel.Width} {panel.AutoSize}");
            _logFile.AppendLine($"windowsFormsHost {windowsFormsHost.Height}  {windowsFormsHost.Width} ");

            return panel;
        }

        private void PutChildOnPanel(Process childProcess, Panel panel)
        {
            IntPtr childHandle = childProcess.MainWindowHandle;
            int oldStyle = GetWindowLong(childHandle, GwlStyle);
            SetWindowLong(childHandle, GwlStyle, (oldStyle | WsChild) & ~WsBorder);
            var parentHandle = new HandleRef(null, panel.Handle);
            var childHandleRef = new HandleRef(null, childHandle);
            SetParent(childHandleRef, parentHandle);
            ShowWindowAsync(childHandleRef, SwShowMaximized);
            _logFile.AppendLine($"PutChildOnPanel: panel {panel.Height}  {panel.Width} ");
        }

        public void RemoveTabItem(int postfix)
        {
            var tabItem = Children.FirstOrDefault(i => (int)(i.Tag) == postfix);
            if (tabItem != null)
                Children.Remove(tabItem);
        }

        public void BringTabItemToFront(int postfix)
        {
            var tabItem = Children.FirstOrDefault(i => (int)(i.Tag) == postfix);
            if (tabItem != null)
                SelectedTabItem = tabItem;
        }
    }
}
