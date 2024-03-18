using System.Diagnostics;

namespace Iit.Fibertest.UtilsNetCore;

public static class DebianCommandLineUtils
{
    public static string GetCommandLineOutput(this string shCommand)
    {
        var cmd = $"-c \"{shCommand}\"";
        ProcessStartInfo startInfo = new() 
        { 
            FileName = "/bin/sh", 
            Arguments = cmd, 
            RedirectStandardOutput = true,
        }; 

        Process proc = new() { StartInfo = startInfo, };
        proc.Start();
        string output = proc.StandardOutput.ReadToEnd();
        proc.WaitForExit();

        return output;
    }

    public static bool ExecuteCommandLine(this string shCommand)
    {
        var cmd = $"-c \"{shCommand}\"";
        ProcessStartInfo startInfo = new() 
        { 
            FileName = "/bin/sh", 
            Arguments = cmd, 
        }; 

        Process proc = new() { StartInfo = startInfo, };
        return proc.Start();
    }
}
