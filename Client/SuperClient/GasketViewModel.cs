using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using Caliburn.Micro;
using System.Windows.Forms.Integration;
using Panel = System.Windows.Forms.Panel;

namespace Iit.Fibertest.SuperClient
{
    public class GasketViewModel : PropertyChangedBase
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

        private Dictionary<int, int> _postfixToTabitem = new Dictionary<int, int>();

        public ObservableCollection<TabItem> Children { get; set; } = new ObservableCollection<TabItem>();

        private int _selectedTabItemIndex;
        public int SelectedTabItemIndex
        {
            get => _selectedTabItemIndex;
            set
            {
                if (_selectedTabItemIndex == value) return;
                _selectedTabItemIndex = value;
                NotifyOfPropertyChange();
            }
        }

        private Panel CreatePanel(int postfix)
        {
            var tabItem = new TabItem() { Header = new ContentControl() };
            Children.Add(tabItem);
            SelectedTabItemIndex = Children.Count -1;
            _postfixToTabitem.Add(postfix, SelectedTabItemIndex);
            
            var windowsFormsHost = new WindowsFormsHost();
            tabItem.Content = windowsFormsHost;

            Panel panel = new Panel();
            windowsFormsHost.Child = panel;

            return panel;
        }

        private void PutChildOnPanel(Process childProcess, Panel panel)
        {
            IntPtr childHandle = childProcess.MainWindowHandle;
            int oldStyle = GetWindowLong(childHandle, GwlStyle);
            SetWindowLong(childHandle, GwlStyle, (oldStyle | WsChild) & ~WsBorder);
            var parentHandle = new HandleRef(null, panel.Handle);
            SetParent(new HandleRef(null, childHandle), parentHandle);
            var childHandleRef = new HandleRef(null, childHandle);
            ShowWindowAsync(childHandleRef, SwShowmaximized);
        }

        public void PutProcessOnPanel(Process childProcess, int postfix)
        {
            var panel = CreatePanel(postfix);

            PutChildOnPanel(childProcess, panel);
        }

        public void RemoveTabItem(int postfix)
        {
            var tabIndex = _postfixToTabitem[postfix];
            Children.RemoveAt(tabIndex);
            _postfixToTabitem.Remove(postfix); 
        }

        public void BringTabItemToFront(int postfix)
        {
            if (_postfixToTabitem.ContainsKey(postfix))
                SelectedTabItemIndex = _postfixToTabitem[postfix];
        }
    }
}
