namespace Iit.Fibertest.UtilsLib
{
    public class NullLog : IMyLog
    {
        public void AppendLine(string message, int offset = 0, string prefix = "")
        {
        }

        public IMyLog AssignFile(string filename)
        {
            return null;
        }

        public void EmptyLine(char ch = ' ')
        {
        }
    }
}