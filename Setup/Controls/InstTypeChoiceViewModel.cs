﻿using System.Windows;
using Caliburn.Micro;

namespace Setup
{
    public class InstTypeChoiceViewModel : PropertyChangedBase
    {
        private Visibility _visibility = Visibility.Collapsed;

        public Visibility Visibility
        {
            get { return _visibility; }
            set
            {
                if (value == _visibility) return;
                _visibility = value;
                NotifyOfPropertyChange();
            }
        }

        public HeaderViewModel HeaderViewModel { get; set; } = new HeaderViewModel();

        public InstTypeChoiceViewModel()
        {
            HeaderViewModel.InBold = "Choose Installation Type";
            HeaderViewModel.Explanation = "Choose which components of IIT Fibertest 2.0 you want to install";
        }
    }
}
