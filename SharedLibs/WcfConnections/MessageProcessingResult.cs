namespace WcfConnections
{
    public enum MessageProcessingResult
    {
        UnknownMessage,
        NothingToReturn,
        ProcessedSuccessfully,
        FailedToProcess,
        TransmittedSuccessfully,
        FailedToTransmit,
    }
}
