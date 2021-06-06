namespace Iit.Fibertest.Dto
{
    public enum DbOptimizationStage
    {
        Starting,
        SorsRemoving,
        TableCompressing,
        ModelAdjusting,

        ModelCreating,

        OptimizationDone,
        SnapshotDone,
    }
}