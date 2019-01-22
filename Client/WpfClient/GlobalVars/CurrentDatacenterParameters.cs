using System;
using Caliburn.Micro;

namespace Iit.Fibertest.Client
{
    public class CurrentDatacenterParameters : PropertyChangedBase
    {
        public string ServerTitle { get; set; }
        public string ServerIp { get; set; }
        public Guid GraphDbVersionId { get; set; }
        public string DatacenterVersion { get; set; }

        private bool _isInGisVisibleMode;
        public bool IsInGisVisibleMode
        {
            get { return _isInGisVisibleMode; }
            set
            {
                if (value == _isInGisVisibleMode) return;
                _isInGisVisibleMode = value;
                NotifyOfPropertyChange();
            }
        }

        public CurrentDatacenterSmtpParameters Smtp { get; set; }
        public int GsmModemComPort { get; set; }
    }
}