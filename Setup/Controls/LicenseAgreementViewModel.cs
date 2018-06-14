﻿using System.Windows;
using Caliburn.Micro;
using Iit.Fibertest.StringResources;

namespace Setup
{
    public class LicenseAgreementViewModel : Screen
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
        public string LicenseRtf { get; set; }

        public LicenseAgreementViewModel()
        {
            HeaderViewModel.InBold = "License Agreement";
            HeaderViewModel.Explanation = "Please review the license terms before installing IIT Fibertest 2.0.";
        }

        protected override void OnInitialize()
        {
            LicenseRtf = Resources.SID_license_en_rtf;

        }
    }
}
