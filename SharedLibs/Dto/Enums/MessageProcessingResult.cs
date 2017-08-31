namespace Dto
{
    public enum MessageProcessingResult
    {
        UnknownMessage,
        UnknownClient,
        UnknownRtu,
        NothingToReturn,
        ProcessedSuccessfully,
        FailedToProcess,

        TransmittedSuccessfully,
        TransmittedSuccessfullyButRtuIsBusy,
        FailedToTransmit,
    }
}
