namespace Iit.Fibertest.UtilsLib
{
    public class NullLog : IMyLog
    {
        public void AppendLine(string message, int offset = 0, string prefix = "")
        {
        }

        public void AssignFile(string filename, int sizeLimitKb, string culture = "ru-RU")
        {
        }

        public void EmptyLine(char ch = ' ')
        {
        }
    }
}