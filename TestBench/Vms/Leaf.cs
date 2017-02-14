using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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

        public ImageSource Pic1 { get; set; }
        public ImageSource Pic2 { get; set; }
        public ImageSource Pic3 { get; set; }
        public ImageSource Pic4 { get; set; }

//        public Visibility Pic3Visibility => LeafType == LeafType.Rtu ? Visibility.Visible : Visibility.Collapsed;
        public Visibility Pic3Visibility => Pic3 != null ? Visibility.Visible : Visibility.Collapsed;
        public Visibility Pic4Visibility => LeafType == LeafType.Rtu ? Visibility.Visible : Visibility.Collapsed;

        public ContextMenu ContextMenu => BuildContextMenu();

        private ContextMenu BuildContextMenu()
        {
            var contextMenu = new ContextMenu();
            contextMenu.Items.Add(new MenuItem() {Header = "Bluh"});
            return contextMenu;
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
