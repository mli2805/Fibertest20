namespace Setup
{
    public enum SetupPages
    {
        Welcome = 0,
        LicenseAgreement = 1,
        InstallationFolder = 2,
        InstTypeChoice = 3,
        ProgressPage,
        Farewell,
    }

    public enum InstallationType
    {
        Client,
        Datacenter,
        RtuManager,
    }
}