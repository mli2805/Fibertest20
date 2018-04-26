﻿using System.Collections.Generic;
using System.Windows.Input;
using Caliburn.Micro;

namespace Iit.Fibertest.Client
{
    public class MenuItemVm : PropertyChangedBase
    {
        public string Header { get; set; }
        public List<MenuItemVm> Children { get; private set; }
        public ICommand Command { get; set; }
        public object CommandParameter { get; set; }

        private bool _isChecked;
        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                if (value == _isChecked) return;
                _isChecked = value;
                NotifyOfPropertyChange();
            }
        }

        public MenuItemVm()
        {
            Children = new List<MenuItemVm>();
        }
    }
}