namespace Dto
{
    public enum MessageProcessingResult
    {
        UnknownMessage,
        UnknownRtu,
        NothingToReturn,
        ProcessedSuccessfully,
        FailedToProcess,

        TransmittedSuccessfully,
        TransmittedSuccessfullyButRtuIsBusy,
        FailedToTransmit,
    }
}
