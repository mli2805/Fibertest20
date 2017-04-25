using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.IitOtdrLibrary;
using Iit.Fibertest.StringResources;
using Optixsoft.SorExaminer.OtdrDataFormat;

namespace Iit.Fibertest.Client
{
    public class LandmarksViewModel : Screen
    {
        private readonly ReadModel _readModel;
        private Trace _trace;
        public ObservableCollection<LandmarkRow> Rows { get; set; }

        public LandmarksViewModel(ReadModel readModel)
        {
            _readModel = readModel;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = string.Format(Resources.SID_Landmarks_of_trace__0_, _trace.Title);
        }

        public void Initialize(Guid traceId)
        {
            _trace = _readModel.Traces.First(t => t.Id == traceId);
            var landmarks = (_trace.PreciseId == Guid.Empty) ?
                new LandmarksGraphParser(_readModel).GetLandmarks(_trace) :
                new LandmarksBaseParser().GetLandmarks(GetBase(_trace.PreciseId));
            Rows = GetRowsFromLandmarks(landmarks);
        }

        private OtdrDataKnownBlocks GetBase(Guid baseId)
        {
            var bytes = File.ReadAllBytes(@"c:\temp\base.sor");
            // TODO get sordata from database
            return SorData.FromBytes(bytes);
        }


        private ObservableCollection<LandmarkRow> GetRowsFromLandmarks(List<Landmark> landmarks)
        {
            return new ObservableCollection<LandmarkRow>(landmarks.Select(l => l.ToRow(GpsInputMode.DegreesMinutesAndSeconds)));
        }
    }
}
