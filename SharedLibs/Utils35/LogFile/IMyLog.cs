namespace Iit.Fibertest.UtilsLib
{
    public interface IMyLog
    {
        void EmptyLine(char ch = ' ');
        void AppendLine(string message, int offset = 0, string prefix = "");
    }
}