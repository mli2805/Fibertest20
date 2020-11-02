using System;
using System.IO;
using System.Windows;
using Caliburn.Micro;
using Iit.Fibertest.UtilsLib;
using Microsoft.Win32;

namespace DirectRtuClient
{
    public class ParseViewModel : Screen
    {
        private readonly IMyLog _logFile;
        private string _resultFileName;
        public string ResultFileName
        {
            get { return _resultFileName; }
            set
            {
                if (Equals(value, _resultFileName)) return;
                _resultFileName = value;
                NotifyOfPropertyChange(() => ResultFileName);
            }
        }

        public ParseViewModel(IMyLog logFile)
        {
            _logFile = logFile;
        }

        public void Parse()
        {
            var bytes = LoadSorFile(ResultFileName);
            var sorData = SorData.FromBytes(bytes);

           _logFile.AppendLine("");
           _logFile.AppendLine(@"Key events");
           _logFile.AppendLine("");
            var keyEvents = sorData.KeyEvents.KeyEvents;
            foreach (var keyEvent in keyEvents)
            {
                _logFile.AppendLine(keyEvent.EventNumber - 1 + @":  " +  keyEvent.Comment);
            }
        }

        public void ChooseResultFilename()
        {
            var fd = new OpenFileDialog();
            fd.Filter = @"Sor files (*.sor)|*.sor";
            fd.InitialDirectory = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\out\"));
            if (fd.ShowDialog() == true)
                ResultFileName = fd.FileName;
        }

        private byte[] LoadSorFile(string filename)
        {
            try
            {
                return File.ReadAllBytes(filename);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return null;
            }
        }

    }
}
