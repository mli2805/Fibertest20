using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Microsoft.Win32;

namespace Iit.Fibertest.Licenser
{
    public class ShellViewModel : Screen, IShell
    {
        public bool HaveRights { get; set; }

        private bool _isEditable;
        public bool IsEditable
        {
            get => _isEditable;
            set
            {
                if (value == _isEditable) return;
                _isEditable = value;
                NotifyOfPropertyChange();
            }
        }

        private int _loadFromFileButtonRow;
        public int LoadFromFileButtonRow
        {
            get => _loadFromFileButtonRow;
            set
            {
                if (value == _loadFromFileButtonRow) return;
                _loadFromFileButtonRow = value;
                NotifyOfPropertyChange();
            }
        }

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

        public ShellViewModel()
        {
            var rr = Environment.GetCommandLineArgs();
            HaveRights = rr.Length > 1 && rr[1] == "ihaverights";

            if (rr.Length > 2)
            {
                var license = new LicenseManager().ReadLicenseFromFile(rr[2]);
                if (license != null)
                    LicenseInFileModel = new LicenseInFileModel(license);
                IsEditable = true;
            }

            LoadFromFileButtonRow = IsEditable ? 0 : 1;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Fibertest 2.0 License maker";
        }

        public void CreateNew()
        {
            LicenseInFileModel = new LicenseInFileModel()
            {
                LicenseId = Guid.NewGuid(),
                IsBasic = true,
                SecurityAdminPassword = Password.Generate(8),
                CreationDate = DateTime.Today,
            };
            IsEditable = true;
            LoadFromFileButtonRow = IsEditable ? 0 : 1;
        }

        public void LoadFromFile()
        {
            var license = new LicenseManager().ReadLicenseFromFileDialog();
            if (license != null)
                LicenseInFileModel = new LicenseInFileModel(license);
            IsEditable = HaveRights;
            LoadFromFileButtonRow = IsEditable ? 0 : 1;
        }


        public void SaveAsFile()
        {
            var license = LicenseInFileModel.ToLicenseInFile();
            var encoded = Cryptography.Encode(license);
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

            var licenseInFile = (LicenseInFile)Cryptography.Decode(encoded);
            LicenseInFileModel = new LicenseInFileModel(licenseInFile);
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

                string filename = Path.Combine(folder, @"LicenseCertificate.pdf");
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