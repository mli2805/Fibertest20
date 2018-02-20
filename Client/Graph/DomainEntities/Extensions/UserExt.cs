namespace Iit.Fibertest.Graph
{
    public static class UserExt
    {
        public static string FlipFlop(string before)
        {
            return string.IsNullOrEmpty(before) ? "" : before.Substring(before.Length - 1, 1) + FlipFlop(before.Substring(0, before.Length - 1));
        }
    }
}