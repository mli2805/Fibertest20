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

        public FiberState State { get; set; }

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

        public ImageSource Pic1
        {
            get
            {
                switch (LeafType)
                {
                    case LeafType.Rtu:
                        return new BitmapImage(new Uri("pack://application:,,,/Resources/LeftPanel/wifi_green.png"));
                    case LeafType.Trace:
                        return new BitmapImage(new Uri("pack://application:,,,/Resources/LeftPanel/Monitoring_Grey.png"));
                    default:
                        return null;
                }
            }
        }

        public ImageSource Pic2
        {
            get
            {
                switch (LeafType)
                {
                    case LeafType.Rtu:
                        return new BitmapImage(new Uri("pack://application:,,,/Resources/LeftPanel/Monitoring_Blue.png"));
                    case LeafType.Trace:
                        return new BitmapImage(new Uri("pack://application:,,,/Resources/LeftPanel/Monitoring_Grey.png"));
                    default:
                        return null;
                }
            }
        }
        public ImageSource Pic3
        {
            get
            {
                switch (LeafType)
                {
                    default:
                        return null;
                }
            }
        }
        public ImageSource Pic4
        {
            get
            {
                switch (LeafType)
                {
                    case LeafType.Trace:
                        return new BitmapImage(new Uri("pack://application:,,,/Resources/LeftPanel/Red_Ball_16.jpg"));
                    default:
                        return null;
                }
            }
        }

        //        public Visibility Pic3Visibility => LeafType == LeafType.Rtu ? Visibility.Visible : Visibility.Collapsed;
        public Visibility Pic3Visibility => Pic3 != null ? Visibility.Visible : Visibility.Collapsed;
        public Visibility Pic4Visibility => LeafType == LeafType.Rtu ? Visibility.Visible : Visibility.Collapsed;

        public  List<MyMenuItem> MyContextMenu => GetMenuItems();
        public  List<MenuItemEx> MyContextMenuEx => GetMenuItemsEx();

        public List<MyMenuItem> GetMenuItems()
        {
            var menu = new List<MyMenuItem>();

            var menuItem = new MyMenuItem() {Header = "Information" };
            var subItem = new MyMenuItem() { Header = "SubItem 1", Command = new ContextMenuAction(SomeMenuItemAction, CanSomeAction), CommandParameter = this };
            menuItem.Children.Add(subItem);
            menu.Add(menuItem);
            var menuItem2 = new MyMenuItem() { Header = "Show RTU", Command = new ContextMenuAction(SomeMenuItemAction, CanSomeAction), CommandParameter = this };
            menu.Add(menuItem2);
            return menu;
        }

        public List<MenuItemEx> GetMenuItemsEx()
        {
            var menu = new List<MenuItemEx>();

            var menuItem = new MenuItemEx() { Header = "Information" };
            var subItem = new MenuItemEx() { Header = "SubItem 1", Command = new ContextMenuAction(SomeMenuItemActionEx, CanSomeAction), CommandParameter = this };
            menuItem.Children.Add(subItem);
            menu.Add(menuItem);
            var menuItem2 = new MenuItemEx() { Header = "Show RTU", Command = new ContextMenuAction(SomeMenuItemActionEx, CanSomeAction), CommandParameter = this };
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