using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using Caliburn.Micro;
using System.Windows.Forms.Integration;
using System.Windows.Media;

namespace Iit.Fibertest.SuperClient
{
    public class GasketViewModel : PropertyChangedBase
    {

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
        private Visibility _tabVisibility;

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

        public Visibility TabVisibility
        {
            get => _tabVisibility;
            set
            {
                if (value == _tabVisibility) return;
                _tabVisibility = value;
                NotifyOfPropertyChange();
            }
        }


        public void PutProcessOnPanel(Process childProcess, int postfix)
        {
            var tabItem = new TabItem() { Header = new ContentControl(), Tag = postfix};
            Children.Add(tabItem);
            SelectedTabItem = tabItem;
            TabVisibility = Visibility.Visible;

            var windowsFormsHost = new WindowsFormsHost() { Background = Brushes.Aquamarine};
            tabItem.Content = windowsFormsHost;

            var panel = CreatePanel();
            PutChildOnPanel(childProcess, panel);

            windowsFormsHost.Child = panel;
        }

        private System.Windows.Forms.Panel CreatePanel()
        {
            var screenHeight = SystemParameters.PrimaryScreenHeight;
            var screenWidth = SystemParameters.PrimaryScreenWidth;

            return new System.Windows.Forms.Panel
            {
                Height = Convert.ToInt16(screenHeight) - 75,
                Width = Convert.ToInt16(screenWidth) - 262
            };
        }

        private void PutChildOnPanel(Process childProcess, System.Windows.Forms.Panel panel)
        {
            IntPtr childHandle = childProcess.MainWindowHandle;
            int oldStyle = GetWindowLong(childHandle, GwlStyle);
            SetWindowLong(childHandle, GwlStyle, (oldStyle | WsChild) & ~WsBorder);
            var parentHandle = new HandleRef(null, panel.Handle);
            var childHandleRef = new HandleRef(null, childHandle);
            SetParent(childHandleRef, parentHandle);
            ShowWindowAsync(childHandleRef, SwShowMaximized);
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
            TabVisibility = tabItem == null ? Visibility.Hidden : Visibility.Visible;
        }
    }
}
