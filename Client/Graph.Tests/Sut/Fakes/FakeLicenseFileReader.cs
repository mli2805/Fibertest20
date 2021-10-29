using Iit.Fibertest.WpfCommonViews;

namespace Graph.Tests
{
    public class FakeLicenseFileChooser : ILicenseFileChooser
    {
        // send here filename instead of initialDirectory
        public string ChooseFilename(string initialDirectory = "")
        {
            return initialDirectory;
        }
    }
}
