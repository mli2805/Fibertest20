using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using PrivateReflectionUsingDynamic;
using WcfConnections.C2DWcfServiceReference;

namespace Iit.Fibertest.Client
{
    public class ClientPoller : PropertyChangedBase
    {
        private readonly IWcfServiceForClient _client;
        public List<object> ReadModels { get; }

        public int CurrentEventNumber { get; private set; }

        public ClientPoller(IWcfServiceForClient client, List<object> readModels)
        {
            _client = client;
            ReadModels = readModels;
        }

        public void Tick()
        {
            foreach (var e in _client.GetEvents(CurrentEventNumber))
            {
                foreach (var m in ReadModels)
                {
                    m.AsDynamic().Apply(e);

                    //
                    var readModel = m as ReadModel;
                    readModel?.NotifyOfPropertyChange(nameof(readModel.JustForNotification));
                    //
                    var treeModel = m as TreeOfRtuModel;
                    treeModel?.NotifyOfPropertyChange(nameof(treeModel.Statistics));
                }
                CurrentEventNumber++;

            }
        }
    }
}