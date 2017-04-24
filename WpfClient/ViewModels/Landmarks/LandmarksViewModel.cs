using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Caliburn.Micro;
using GMap.NET;
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
                GetLandmarksFromGraph() :
                GetLandmarksFromBase(GetBase(_trace.PreciseId));
            Rows = GetRowsFromLandmarks(landmarks);
        }

        private OtdrDataKnownBlocks GetBase(Guid baseId)
        {
            var bytes = File.ReadAllBytes(@"c:\temp\base.sor");
            // TODO get sordata from database
            return SorData.FromBytes(bytes);
        }

        private List<Landmark> GetLandmarksFromBase(OtdrDataKnownBlocks sorData)
        {
            return new List<Landmark>();
        }

        private ObservableCollection<LandmarkRow> GetRowsFromLandmarks(List<Landmark> landmarks)
        {
            return new ObservableCollection<LandmarkRow>(landmarks.Select(l => l.ToRow(GpsInputMode.DegreesMinutesAndSeconds)));
        }

        private List<Landmark> GetLandmarksFromGraph()
        {
            var list = _trace.Nodes.Select((t, i) => CombineLandmark(t, _trace.Equipments[i], i)).ToList();
            for (var i = 1; i < list.Count; i++)
                list[i].Location = list[i].GpsCoors.GetDistanceKm(list[i - 1].GpsCoors) + list[i-1].Location;
            return list;
        }

        
        private Landmark CombineLandmark(Guid nodeId, Guid equipmentId, int number)
        {
            var node = _readModel.Nodes.First(n => n.Id == nodeId);
            var result = new Landmark()
            {
                Number = number,
                NodeTitle = node.Title,
                EventNumber = -1,
                GpsCoors = new PointLatLng(node.Latitude, node.Longitude)
            };

            if (number == 0)
            {
                var rtu = _readModel.Rtus.First(e => e.Id == equipmentId);
                result.EquipmentTitle = rtu.Title;
                result.EquipmentType = EquipmentType.Rtu;
            }
            else
            {
                if (equipmentId != Guid.Empty)
                {
                    var equipment = _readModel.Equipments.First(e => e.Id == equipmentId);
                    result.EquipmentTitle = equipment.Title;
                    result.EquipmentType = equipment.Type;
                }
                else result.EquipmentType = EquipmentType.Well;
            };
            return result;
        }


    }
}
