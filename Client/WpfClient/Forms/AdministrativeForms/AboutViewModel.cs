using System.Collections.ObjectModel;
using System.Diagnostics;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class RtuVersion
    {
        public string Title { get; set; }
        public string Version { get; set; }

        public RtuVersion(string title, string version)
        {
            Title = title;
            Version = version;
        }
    }

    public class AboutViewModel : Screen
    {
        private readonly Model _readModel;
        public string ServerVersion { get; set; }
        public string ClientVersion { get; set; }

        public ObservableCollection<RtuVersion> Rtus { get;set; }= new ObservableCollection<RtuVersion>();

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
                Rtus.Add(new RtuVersion(rtu.Title, rtu.Version));
        }

        public void Close() { TryClose(); }
    }
}
