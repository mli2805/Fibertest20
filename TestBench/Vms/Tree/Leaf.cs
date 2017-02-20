using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.TestBench.Properties;

namespace Iit.Fibertest.TestBench
{
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

        public ImageSource MonitoringPictogram => MonitoringState.GetPictogram();
        public ImageSource BopPictogram => BopState.GetPictogram();
        public ImageSource MainChannelPictogram => MainChannelState.GetPictogram();
        public ImageSource ReserveChannelPictogram => ReserveChannelState.GetPictogram();
        public ImageSource TraceStatePictogram => TraceState.GetPictogram();

        public Visibility BopVisibility => LeafType == LeafType.Rtu ? Visibility.Visible : Visibility.Collapsed;
        public Visibility MainChannelVisibility => LeafType == LeafType.Rtu ? Visibility.Visible : Visibility.Collapsed;
        public Visibility ReserveChannelVisibility => LeafType == LeafType.Rtu ? Visibility.Visible : Visibility.Collapsed;
        public Visibility TraceStateVisibility => LeafType == LeafType.Trace ? Visibility.Visible : Visibility.Collapsed;

        public  List<MenuItemVm> MyContextMenu => GetMenuItems();

        public List<MenuItemVm> GetMenuItems()
        {
            var menu = new List<MenuItemVm>();

            var menuItem = new MenuItemVm() {Header = Resources.SID_Information };
            var subItem = new MenuItemVm() { Header = Resources.SID_Trace, Command = new ContextMenuAction(SomeMenuItemAction, CanSomeAction), CommandParameter = this };
            menuItem.Children.Add(subItem);
            menu.Add(menuItem);
            var menuItem2 = new MenuItemVm() { Header = Resources.SID_Show_RTU, Command = new ContextMenuAction(SomeMenuItemAction, CanSomeAction), CommandParameter = this };
            menu.Add(menuItem2);
            return menu;
        }

        private bool CanSomeAction(object param) { return true;}
        private void SomeMenuItemAction(object param)
        {
            Console.WriteLine($"owner is {Title}");
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
}