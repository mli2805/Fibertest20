using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    [DataContract]
    public class DbOptimizationProgressDto
    {
        [DataMember]
        public DbOptimizationStage Stage { get; set; }

        [DataMember]
        public int MeasurementsChosenForDeletion { get; set; }

        [DataMember]
        public double Copied { get; set; }

  [DataMember]
        public double Recreated { get; set; }

        [DataMember]
        public double OldSizeGb { get; set; }
        [DataMember]
        public double NewSizeGb { get; set; }
    }
}