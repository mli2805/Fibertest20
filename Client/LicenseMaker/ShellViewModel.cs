using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.WpfCommonViews;
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
            LicenseInFile license = new LicenseInFile()
            {
                LicenseId = LicenseInFileModel.LicenseId,
                Owner = LicenseInFileModel.Owner,
                IsReplacementLicense = !LicenseInFileModel.IsIncremental,
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
                CreationDate = LicenseInFileModel.CreationDate,
            };
            return new LicenseManager().Encode(license);
        }

        public void ToPdf()
        {
            var provider = new PdfCertificateProvider();
            var pdfDoc = provider.Create(LicenseInFileModel);
            if (pdfDoc == null) return;

            try
            {
                var folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\Reports");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                string filename = Path.Combine(folder, @"MonitoringSystemComponentsReport.pdf");
                pdfDoc.Save(filename);
                Process.Start(filename);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        public void Close()
        {
            TryClose();
        }
    }
}