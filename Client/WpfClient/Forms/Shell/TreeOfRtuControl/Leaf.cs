﻿using System;
using System.Collections.Generic;
using System.Windows.Media;
using Caliburn.Micro;
using Iit.Fibertest.WcfServiceForClientInterface;

namespace Iit.Fibertest.Client
{
   public class Leaf : PropertyChangedBase
    {
        public readonly ReadModel ReadModel;
        public readonly IWindowManager WindowManager;
        public readonly IWcfServiceForClient C2DWcfManager;
        public Guid Id { get; set; }

        private string _title;
        public string Title
        {
            get => _title;
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
            get => _color;
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
            get => _postOffice;
            private set
            {
                if (Equals(value, _postOffice)) return;
                _postOffice = value;
                NotifyOfPropertyChange();
            }
        }

        public List<MenuItemVm> MyContextMenu => GetMenuItems();

        protected virtual List<MenuItemVm> GetMenuItems() { return null; }

        public Leaf Parent { get; set; }

        protected Leaf(ReadModel readModel, IWindowManager windowManager, IWcfServiceForClient c2DWcfManager, PostOffice postOffice)
        {
            ReadModel = readModel;
            WindowManager = windowManager;
            C2DWcfManager = c2DWcfManager;
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