using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Iit.Fibertest.UtilsLib
{
    public static class Utils
    {
        public static string FileNameForSure(string subDir, string filename, bool isBoomNeeded, bool isSubDirAbsolute = false)
        {
            try
            {
                string folder = isSubDirAbsolute
                    ? subDir
                    : Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, subDir));
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                var fullPath = Path.GetFullPath(Path.Combine(folder, filename));
                if (File.Exists(fullPath))
                    return fullPath;
                using (FileStream fs = File.Create(fullPath))
                {
                    if (isBoomNeeded)
                    { fs.WriteByte(239); fs.WriteByte(187); fs.WriteByte(191); }
                }
                return fullPath;
            }
            catch (COMException e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        public static TimeSpan GetUpTime()
        {
            int tickCount = Environment.TickCount; // unfortunately INT, so after 24 days tickCount is invalid
            return TimeSpan.FromMilliseconds(tickCount);
        }

        // public static TimeSpan GetUpTime()
        // {
        //     using var upTime = new PerformanceCounter("System", "System Up Time");
        //     upTime.NextValue();       //Call this an extra time before reading its value
        //     return TimeSpan.FromSeconds(upTime.NextValue());
        // }

        // public static TimeSpan GetUpTime()
        // {
        //     return TimeSpan.FromMilliseconds(GetTickCount64());
        // }

        // [DllImport("kernel32")]
        // static extern UInt64 GetTickCount64();
    }
}