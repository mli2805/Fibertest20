using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.TestBench.Properties;

namespace Iit.Fibertest.TestBench
{
    public enum LeafType
    {
        DataCenter,
        Rtu,
        Bop,
        Trace
    }

    public class Leaf : PropertyChangedBase, ITreeViewItemModel
    {
        private string _title;

        public string Title
        {
            get { return _title; }
            set
            {
                if (value == _title) return;
                _title = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(SelectedValuePath));
                NotifyOfPropertyChange(nameof(DisplayValuePath));
            }
        }

        public Leaf Parent { get; set; }
        public ObservableCollection<Leaf> Children { get; set; } = new ObservableCollection<Leaf>();

        public Guid Id { get; set; }
        public LeafType LeafType { get; set; }
        public int PortNumber { get; set; }

        public MonitoringState MonitoringState { get; set; }
        public RtuPartState BopState { get; set; }
        public RtuPartState MainChannelState { get; set; }
        public RtuPartState ReserveChannelState { get; set; }
        public FiberState TraceState { get; set; }

        private Brush _color;

        public Brush Color
        {
            get { return _color; }
            set
            {
                if (Equals(value, _color)) return;
                _color = value;
                NotifyOfPropertyChange();
            }
        }

        public ImageSource MonitoringPictogram
        {
            get
            {
                switch (MonitoringState)
                {
                    case MonitoringState.Off:
                        return new BitmapImage(new Uri("pack://application:,,,/Resources/LeftPanel/EmptySquare.png"));
                    case MonitoringState.OffButReady:
                        return new BitmapImage(new Uri("pack://application:,,,/Resources/LeftPanel/GreySquare.png"));
                    case MonitoringState.On:
                        return new BitmapImage(new Uri("pack://application:,,,/Resources/LeftPanel/BlueSquare.png"));
                    default:
                        return null;
                }
            }
        }

        public ImageSource BopPictogram
        {
            get
            {
                switch (BopState)
                {
                    case RtuPartState.Broken:
                        return new BitmapImage(new Uri("pack://application:,,,/Resources/LeftPanel/RedSquare.png"));
                    case RtuPartState.None:
                        return new BitmapImage(new Uri("pack://application:,,,/Resources/LeftPanel/EmptySquare.png"));
                    case RtuPartState.Normal:
                        return new BitmapImage(new Uri("pack://application:,,,/Resources/LeftPanel/GreenSquare.png"));
                    default:
                        return null;
                }
            }
        }
        public ImageSource MainChannelPictogram
        {
            get
            {
                switch (MainChannelState)
                {
                    case RtuPartState.Broken:
                        return new BitmapImage(new Uri("pack://application:,,,/Resources/LeftPanel/RedSquare.png"));
                    case RtuPartState.None:
                        return new BitmapImage(new Uri("pack://application:,,,/Resources/LeftPanel/EmptySquare.png"));
                    case RtuPartState.Normal:
                        return new BitmapImage(new Uri("pack://application:,,,/Resources/LeftPanel/GreenSquare.png"));
                    default:
                        return null;
                }
            }
        }
        public ImageSource ReserveChannelPictogram
        {
            get
            {
                switch (ReserveChannelState)
                {
                    case RtuPartState.Broken:
                        return new BitmapImage(new Uri("pack://application:,,,/Resources/LeftPanel/RedSquare.png"));
                    case RtuPartState.None:
                        return new BitmapImage(new Uri("pack://application:,,,/Resources/LeftPanel/EmptySquare.png"));
                    case RtuPartState.Normal:
                        return new BitmapImage(new Uri("pack://application:,,,/Resources/LeftPanel/GreenSquare.png"));
                    default:
                        return null;
                }
            }
        }
        public ImageSource TraceStatePictogram
        {
            get
            {
                switch (TraceState)
                {
                    case FiberState.NotJoined:
                        return new BitmapImage(new Uri("pack://application:,,,/Resources/LeftPanel/EmptySquare.png"));
                    case FiberState.Ok:
                        return new BitmapImage(new Uri("pack://application:,,,/Resources/LeftPanel/EmptySquare.png"));
                    case FiberState.Minor:
                        return new BitmapImage(new Uri("pack://application:,,,/Resources/LeftPanel/EmptySquare.png"));
                    case FiberState.Major:
                        return new BitmapImage(new Uri("pack://application:,,,/Resources/LeftPanel/EmptySquare.png"));
                    case FiberState.User:
                        return new BitmapImage(new Uri("pack://application:,,,/Resources/LeftPanel/GreenSquare.png"));
                    case FiberState.Critical:
                    case FiberState.FiberBreak:
                    case FiberState.NoFiber:
                        return new BitmapImage(new Uri("pack://application:,,,/Resources/LeftPanel/RedSquare.png"));
                    default:
                        return null;
                }
            }
        }

        public Visibility BopVisibility => LeafType == LeafType.Rtu ? Visibility.Visible : Visibility.Collapsed;
        public Visibility MainChannelVisibility => LeafType == LeafType.Rtu ? Visibility.Visible : Visibility.Collapsed;
        public Visibility ReserveChannelVisibility => LeafType == LeafType.Rtu ? Visibility.Visible : Visibility.Collapsed;
        public Visibility TraceStateVisibility => LeafType == LeafType.Trace ? Visibility.Visible : Visibility.Collapsed;

        public  List<MyMenuItem> MyContextMenu => GetMenuItems();
        public  List<MenuItemEx> MyContextMenuEx => GetMenuItemsEx();

        public List<MyMenuItem> GetMenuItems()
        {
            var menu = new List<MyMenuItem>();

            var menuItem = new MyMenuItem() {Header = Resources.SID_Information };
            var subItem = new MyMenuItem() { Header = Resources.SID_Trace, Command = new ContextMenuAction(SomeMenuItemAction, CanSomeAction), CommandParameter = this };
            menuItem.Children.Add(subItem);
            menu.Add(menuItem);
            var menuItem2 = new MyMenuItem() { Header = Resources.SID_Show_RTU, Command = new ContextMenuAction(SomeMenuItemAction, CanSomeAction), CommandParameter = this };
            menu.Add(menuItem2);
            return menu;
        }

        public List<MenuItemEx> GetMenuItemsEx()
        {
            var menu = new List<MenuItemEx>();

            var menuItem = new MenuItemEx() { Header = Resources.SID_Information };
            var subItem = new MenuItemEx() { Header = Resources.SID_Trace, Command = new ContextMenuAction(SomeMenuItemActionEx, CanSomeAction), CommandParameter = this };
            menuItem.Children.Add(subItem);
            menu.Add(menuItem);
            var menuItem2 = new MenuItemEx() { Header = Resources.SID_Show_RTU, Command = new ContextMenuAction(SomeMenuItemActionEx, CanSomeAction), CommandParameter = this };
            menu.Add(menuItem2);
            return menu;
        }


        private bool CanSomeAction(object param) { return true;}
        private void SomeMenuItemAction(object param)
        {
            Console.WriteLine($"owner is {Title}");
        }
        private void SomeMenuItemActionEx(object param)
        {
            var item = (MenuItemEx) param;
            Console.WriteLine($"menu item {item.Header} was clicked");
        }

        #region implementation of ITreeViewItemModel

        public string SelectedValuePath => Title;
        public string DisplayValuePath => Title;

        public bool IsExpanded { get; set; }
        public bool IsSelected { get; set; }

        private IEnumerable<Leaf> GetAscendingHierarchy()
        {
            var account = this;

            yield return account;
            while (account.Parent != null)
            {
                yield return account.Parent;
                account = account.Parent;
            }
        }

        public IEnumerable<ITreeViewItemModel> GetHierarchy()
        {
            return GetAscendingHierarchy().Reverse();
        }

        public IEnumerable<ITreeViewItemModel> GetChildren()
        {
            return Children;
        }

        #endregion
    }

    public class MyMenuItem
    {
        public string Header { get; set; }
        public List<MyMenuItem> Children { get; private set; }
        public ICommand Command { get; set; }
        public object CommandParameter { get; set; }

        public MyMenuItem()
        {
            Children = new List<MyMenuItem>();
        }
    }

    public class MenuItemEx : MenuItem
    {
        public List<MenuItemEx> Children { get; private set; } = new List<MenuItemEx>();

    }
}