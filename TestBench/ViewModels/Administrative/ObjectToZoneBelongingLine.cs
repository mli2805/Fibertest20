using System;
using System.Collections.ObjectModel;
using Caliburn.Micro;

namespace Iit.Fibertest.TestBench
{
    public class ObjectToZoneBelongingLine : PropertyChangedBase
    {
        public Guid ObjectId { get; set; }
        public String ObjectTitle { get; set; }
        public bool DoesBelongDefaultZone { get; private set; } = true;
        public ObservableCollection<bool> Belongings { get; set; } = new ObservableCollection<bool>();
    }
}