using System;
using System.IO;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.WpfCommonViews;
using Microsoft.Win32;

namespace Iit.Fibertest.Licenser
{
    public class ShellViewModel : Screen, IShell
    {
        public bool HaveRights { get; set; }
        public bool AppJustOpened { get; set; }

        private bool _showPdfAndSave;
        public bool ShowPdfAndSave
        {
            get => _showPdfAndSave;
            set
            {
                if (value == _showPdfAndSave) return;
                _showPdfAndSave = value;
                NotifyOfPropertyChange();
            }
        }

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
            AppJustOpened = true;
            HaveRights = false;
            IsEditable = false;
            LoadFromFileButtonRow = 1;
            ShowPdfAndSave = false;

            var rr = Environment.GetCommandLineArgs();
            HaveRights = rr.Length > 1 && rr[1] == "ihaverights";

            // второй параметр - файл лицензии, тогда сразу открываем ее для редактирования
            if (HaveRights && rr.Length > 2)
            {
                var licFileDecoder = new LicenseFromFileDecoder(new WindowManager());
                var license = licFileDecoder.Decode(rr[2]);
                if (license != null)
                    LicenseInFileModel = new LicenseInFileModel(license);
                LoadFromFileButtonRow = 0;
                AppJustOpened = false;
                ShowPdfAndSave = true;
            }
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
                IsStandart = true,
                IsBasic = true,
                SecurityAdminPassword = Password.Generate(8),
                CreationDate = DateTime.Today,
            };
            IsEditable = true;
            ShowPdfAndSave = true;
            LoadFromFileButtonRow = 0;
        }

        public void LoadFromFile()
        {
            var licenseFromFileDecoder = new LicenseFromFileDecoder(new WindowManager());
            var licFileReader = new LicenseFileChooser();
            var license = licenseFromFileDecoder.Decode(licFileReader.ChooseFilename());

            if (license != null)
                LicenseInFileModel = new LicenseInFileModel(license);
            IsEditable = false; // существующий файл никому нельзя редактировать, можно только создавать новый
            ShowPdfAndSave = true;
            LoadFromFileButtonRow = 0;
        }


        public void SaveAsFile()
        {
            if (LicenseInFileModel.IsIncremental)
                LicenseInFileModel.IsMachineKeyRequired = false;

            var license = LicenseInFileModel.ToLicenseInFile();
            var encoded = Cryptography.Encode(license);

            SaveFileDialog dlg = new SaveFileDialog();
            dlg.FileName = LicenseInFileModel.LicenseKey; // Default file name
            dlg.DefaultExt = ".lic";
            dlg.Filter = "License file (*.lic)|*.lic";

            if (dlg.ShowDialog() == true)
            {
                string filename = dlg.FileName;
                File.WriteAllBytes(filename, encoded);
            }

            IsEditable = false;
        }

        public void ToPdf()
        {
            var provider = new PdfCertificateProvider();
            var pdfDoc = provider.Create(LicenseInFileModel);
            if (pdfDoc == null) return;
            PdfExposer.Show(pdfDoc, @"LicenseCertificate.pdf", new WindowManager());
        }

        public void Close()
        {
            TryClose();
        }
    }
}