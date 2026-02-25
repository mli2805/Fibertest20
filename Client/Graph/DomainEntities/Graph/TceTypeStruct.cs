using System;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Graph
{
    [Serializable]
    public struct TceTypeStruct : IEquatable<TceTypeStruct>
    {
        public int Id { get; set; }
        public bool IsVisible { get; set; } // show only models user has
        public string Model { get; set; }
        public TceMaker Maker { get; set; }
        public string SoftwareVersion { get; set; }
        public string Code { get; set; } // for pretty parser switch
        public int SlotCount => SlotPositions.Length;
        public int[] SlotPositions { get; set; }
        public int GponInterfaceNumerationFrom { get; set; }
        public string Comment { get; set; }

        public string TypeTitle => $@"{Maker} {Model} {SoftwareVersion}";

        public bool Equals(TceTypeStruct other)
        {
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            return obj is TceTypeStruct other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Id;
        }
    }
}
