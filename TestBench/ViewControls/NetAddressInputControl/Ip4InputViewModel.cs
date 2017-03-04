namespace Iit.Fibertest.TestBench
{
    public class Ip4InputViewModel
    {
        public string[] Parts { get; set; }

        public Ip4InputViewModel(string ip4Address)
        {
            Parts = ip4Address.Split('.');
        }

        public string GetString()
        {
            return $@"{Parts[0]}.{Parts[1]}.{Parts[2]}.{Parts[3]}";
        }
    }
}
