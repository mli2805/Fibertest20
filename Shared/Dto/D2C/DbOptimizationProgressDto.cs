using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    [DataContract]
    public class DbOptimizationProgressDto
    {
        [DataMember]
        public string Username { get; set; }

        [DataMember]
        public DbOptimizationStage Stage { get; set; }

        [DataMember]
        public int MeasurementsChosenForDeletion { get; set; }

        [DataMember]
        public double TableOptimizationPercent { get; set; }

        [DataMember]
        public double EventsReplayed { get; set; }

        [DataMember]
        public double OldSizeGb { get; set; }
        [DataMember]
        public double NewSizeGb { get; set; }
    }
}