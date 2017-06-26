namespace Iit.Fibertest.DirectCharonLibrary
{
    public enum CharonLogLevel
    {
        Off = 0,
        PublicCommands = 1,
        BasicCommands = 2,
        TransmissionCommands = 3,
    }

    public enum CharonOperationResult
    {
        OtdrError = -9,
        AdditionalOtauError = -2,
        MainOtauError = -1,
        LogicalError = 0,
        Ok = 1,
    }
}