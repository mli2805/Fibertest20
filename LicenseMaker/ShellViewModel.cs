using System.IO;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Microsoft.Win32;

namespace LicenseMaker
{
    public class ShellViewModel : Screen, IShell
    {
        private LicenseModel _licenseModel = new LicenseModel();
        public LicenseModel LicenseModel
        {
            get => _licenseModel;
            set
            {
                if (value == _licenseModel) return;
                _licenseModel = value;
                NotifyOfPropertyChange();
            }
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Fibertest 2.0 License maker";
        }

        public void LoadFromFile()
        {
            var license = new LicenseManager().ReadLicenseFromFile();
            if (license != null)
                LicenseModel = new LicenseModel(license);
        }

       
        public void SaveAsFile()
        {
            var encoded = EncodeLicense();
            SaveLicenseAsFile(encoded);
        }

        private static void SaveLicenseAsFile(byte[] encoded)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.FileName = "Fibertest20"; // Default file name
            dlg.DefaultExt = ".lic"; 
            dlg.Filter = "License file (.lic)|*.lic"; 

            if (dlg.ShowDialog() == true)
            {
                string filename = dlg.FileName;
                File.WriteAllBytes(filename, encoded);
            }
        }

        private byte[] EncodeLicense()
        {
            License _license = new License()
            {
                Owner = LicenseModel.Owner,
                RtuCount = LicenseModel.RtuCount,
                ClientStationCount = LicenseModel.ClientStationCount,
                SuperClientEnabled = LicenseModel.SuperClientEnabled,
            };
            return new LicenseManager().Encode(_license);
        }

        public void Close()
        {
            TryClose();
        }
    }
}