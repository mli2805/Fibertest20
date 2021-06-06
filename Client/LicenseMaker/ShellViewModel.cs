using System;
using System.IO;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Microsoft.Win32;

namespace LicenseMaker
{
    public class ShellViewModel : Screen, IShell
    {
        private LicenseInFileModel _licenseInFileModel = new LicenseInFileModel();

        public LicenseInFileModel LicenseInFileModel
        {
            get => _licenseInFileModel;
            set
            {
                if (value == _licenseInFileModel) return;
                _licenseInFileModel = value;
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
                LicenseInFileModel = new LicenseInFileModel(license);
        }


        public void SaveAsFile()
        {
            var encoded = EncodeLicense();
            SaveLicenseAsFile(encoded);
        }

        private void SaveLicenseAsFile(byte[] encoded)
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

            var licenseInFile = new LicenseManager().Decode(encoded);
            LicenseInFileModel = new LicenseInFileModel(licenseInFile);
        }

        private byte[] EncodeLicense()
        {
            LicenseInFile _license = new LicenseInFile()
            {
                LicenseId = Guid.NewGuid(),
                Owner = LicenseInFileModel.Owner,
                RtuCount = new LicenseParameterInFile()
                {
                    Value = LicenseInFileModel.RtuCount,
                    Term = LicenseInFileModel.RtuCountTerm,
                    IsTermInYears = LicenseInFileModel.RtuCountTermUnit == LicenseInFileModel.TermUnit.First(),
                },
                ClientStationCount = new LicenseParameterInFile()
                {
                    Value = LicenseInFileModel.ClientStationCount,
                    Term = LicenseInFileModel.ClientStationTerm,
                    IsTermInYears = LicenseInFileModel.ClientStationTermUnit == LicenseInFileModel.TermUnit.First(),
                }, 
                WebClientCount = new LicenseParameterInFile()
                {
                    Value = LicenseInFileModel.WebClientCount,
                    Term = LicenseInFileModel.WebClientTerm,
                    IsTermInYears = LicenseInFileModel.WebClientTermUnit == LicenseInFileModel.TermUnit.First(),
                },
                SuperClientStationCount = new LicenseParameterInFile()
                {
                    Value = LicenseInFileModel.SuperClientStationCount,
                    Term = LicenseInFileModel.SuperClientTerm,
                    IsTermInYears = LicenseInFileModel.SuperClientTermUnit == LicenseInFileModel.TermUnit.First(),
                },
            };
            return new LicenseManager().Encode(_license);
        }

        public void Close()
        {
            TryClose();
        }
    }
}