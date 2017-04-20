using System;
using System.Collections.Generic;
using System.Windows.Media;
using Caliburn.Micro;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class Leaf : PropertyChangedBase
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
                NotifyOfPropertyChange(nameof(Name));
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

        public virtual string Name { get; set; }

        private PostOffice _postOffice;
        public PostOffice PostOffice
        {
            get { return _postOffice; }
            set
            {
                if (Equals(value, _postOffice)) return;
                _postOffice = value;
                NotifyOfPropertyChange();
            }
        }

        public List<MenuItemVm> MyContextMenu => GetMenuItems();

        protected virtual List<MenuItemVm> GetMenuItems() { return null; }
        protected bool CanSomeAction(object param) { return true; }

        public Leaf Parent { get; set; }

        protected Leaf(ReadModel readModel, IWindowManager windowManager, Bus bus, PostOffice postOffice)
        {
            ReadModel = readModel;
            WindowManager = windowManager;
            Bus = bus;
            PostOffice = postOffice;
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (value == _isSelected) return;
                _isSelected = value;
                NotifyOfPropertyChange();
            }
        }

        private bool _isExpanded;
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                if (value == _isExpanded) return;
                _isExpanded = value;
                NotifyOfPropertyChange();
            }
        }
    }
}