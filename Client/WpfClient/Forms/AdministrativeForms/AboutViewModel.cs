using System.Collections.ObjectModel;
using System.Diagnostics;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class RtuLine
    {
        public string Title { get; set; }
        public string Model { get; set; }
        public string Serial { get; set; }
        public string Version { get; set; }

        public RtuLine(string title, string model, string serial, string version)
        {
            Title = title;
            Model = model;
            Serial = serial;
            Version = version;
        }
    }

    public class AboutViewModel : Screen
    {
        private readonly Model _readModel;
        public string ServerVersion { get; set; }
        public string ClientVersion { get; set; }

        public ObservableCollection<RtuLine> Rtus { get;set; }= new ObservableCollection<RtuLine>();

        public AboutViewModel(CurrentDatacenterParameters currentDatacenterParameters, Model readModel)
        {
            _readModel = readModel;
            ServerVersion = currentDatacenterParameters.DatacenterVersion;

            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            ClientVersion = fvi.FileVersion;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_About;

            Rtus.Clear();
            foreach (var rtu in _readModel.Rtus)
                Rtus.Add(new RtuLine(rtu.Title, rtu.Mfid, rtu.Serial, rtu.RtuMaker == RtuMaker.IIT ? rtu.Version : rtu.Version2));
        }

        public void Close() { TryClose(); }
    }
}
