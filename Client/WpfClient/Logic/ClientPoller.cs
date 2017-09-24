using System.Collections.Generic;
using Caliburn.Micro;
using Newtonsoft.Json;
using PrivateReflectionUsingDynamic;
//using WcfConnections.C2DWcfServiceReference;

namespace Iit.Fibertest.Client
{
    public class ClientPoller : PropertyChangedBase
    {

        private static readonly JsonSerializerSettings JsonSerializerSettings =
            new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            };
        private readonly WcfServiceForClientLibrary.IWcfServiceForClient _client;
        public List<object> ReadModels { get; }

        public int CurrentEventNumber { get; private set; }

        public ClientPoller(WcfServiceForClientLibrary.IWcfServiceForClient client, List<object> readModels)
        {
            _client = client;
            ReadModels = readModels;
        }

        public void Tick()
        {
            foreach (var json in _client.GetEvents(CurrentEventNumber))
            {
                var e = JsonConvert.DeserializeObject(json, JsonSerializerSettings);
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