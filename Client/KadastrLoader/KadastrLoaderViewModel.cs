using System;
using Caliburn.Micro;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;
using Microsoft.Win32;

namespace KadastrLoader 
{
    public class KadastrLoaderViewModel : Screen, IShell
    {
        private string _selectedFolder;
        public string SelectedFolder
        {
            get => _selectedFolder;
            set
            {
                if (value == _selectedFolder) return;
                _selectedFolder = value;
                NotifyOfPropertyChange();
            }
        }

        public KadastrLoaderViewModel()
        {
            CreateKadastrDbIfNeeded();
        }

        private void CreateKadastrDbIfNeeded()
        {

        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Load_from_Kadastr;
        }

        public void SelectFolder()
        {
            var dlg = new OpenFileDialog
            {
                InitialDirectory = FileOperations.GetParentFolder(AppDomain.CurrentDomain.BaseDirectory),
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
            };
            if (dlg.ShowDialog() == true)
                SelectedFolder = FileOperations.GetParentFolder(dlg.FileName);
        }

        public void Close()
        {
            TryClose();
        }
    }
}