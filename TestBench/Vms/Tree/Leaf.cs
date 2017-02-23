using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
using Caliburn.Micro;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.TestBench
{
    public class Leaf : PropertyChangedBase, ITreeViewItemModel
    {
        protected readonly ReadModel ReadModel;
        protected readonly IWindowManager WindowManager;
        protected readonly Bus Bus;
        public Guid Id { get; set; }

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

        public  List<MenuItemVm> MyContextMenu => GetMenuItems();

        protected virtual List<MenuItemVm> GetMenuItems() { return null; }

        public Leaf Parent { get; set; }
        public ObservableCollection<Leaf> Children { get; set; } = new ObservableCollection<Leaf>();

        public Leaf(ReadModel readModel, IWindowManager windowManager, Bus bus)
        {
            ReadModel = readModel;
            WindowManager = windowManager;
            Bus = bus;
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