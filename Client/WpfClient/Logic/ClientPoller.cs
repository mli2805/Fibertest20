using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using PrivateReflectionUsingDynamic;

namespace Iit.Fibertest.Client
{
    public class ClientPoller : PropertyChangedBase
    {
        private readonly Db _db;
        public List<object> ReadModels { get; }

        public int CurrentEventNumber { get; private set; }

        public ClientPoller(Db db, List<object> readModels)
        {
            _db = db;
            ReadModels = readModels;
        }

        public void Tick()
        {
            foreach (var e in _db.Events.Skip(CurrentEventNumber))
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

            }
            CurrentEventNumber = _db.Events.Count;
        }
    }
}