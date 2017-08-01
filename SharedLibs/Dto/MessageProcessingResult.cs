namespace Dto
{
    public enum MessageProcessingResult
    {
        UnknownMessage,
        NothingToReturn,
        ProcessedSuccessfully,
        FailedToProcess,

        TransmittedSuccessfully,
        TransmittedSuccessfullyButRtuIsBusy,
        FailedToTransmit,
    }
}
