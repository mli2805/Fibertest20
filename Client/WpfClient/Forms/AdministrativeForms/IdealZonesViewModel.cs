using System;
using System.Collections.Generic;
using Caliburn.Micro;

namespace Iit.Fibertest.Client
{
    public class ObjectToZonesModel
    {
        public string ObjectTitle { get; set; }
        public Guid ObjectId { get; set; }
        public bool IsRtu { get; set; }

        public bool[] Zones { get; set; }

    }
    public class IdealZonesViewModel : Screen
    {
        public List<ObjectToZonesModel> Rows { get; set; } = new List<ObjectToZonesModel>();

        public IdealZonesViewModel()
        {
            Initialize();
        }

        private void Initialize()
        {
            var zoneCount = 3;
            for (int i = 0; i < 10; i++)
            {
                var objectModel = new ObjectToZonesModel()
                {
                    ObjectTitle = $@"object {i}",
                    IsRtu = true,
                    Zones = new []{true, false, true},
                };
                Rows.Add(objectModel);
            }
        }
    }
}
