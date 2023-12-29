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

        public static string ToDiskSpaceSize(this long number)
        {
            return number < 1000
                ? $"{number} B"
                : number / 1000 < 1000
                    ? $"{number / 1000:##,###.#} KB"
                    : number / 1000 / 1000 < 1000
                        ? $"{number / 1000 / 1000:##,###.#} MB"
                        : number / 1000 / 1000 / 1000 < 1000
                            ? $"{number / 1000 / 1000 / 1000:##,###.#} GB"
                            : $"{number / 1000 / 1000 / 1000 / 1000:##,###.#} TB";
        }
    }
}