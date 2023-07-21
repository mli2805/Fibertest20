using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Iit.Fibertest.SuperClient
{
    public static class Utils
    {
        public static List<string> GetClientVersions()
        {
            try
            {
                return GetClients().ToList();
            }
            catch (Exception e)
            {
                return new List<string>() { e.Message, e.InnerException?.Message };
            }
        }


        private static IEnumerable<string> GetClients()
        {
            string basePath = @"C:\Iit-Fibertest\";
            string pattern = @"client.*";

                if (!Directory.Exists(basePath))
                {
                    yield return $"Can't find base folder {basePath}";
                }
                else
                {
                    var list = Directory.GetDirectories(basePath, pattern);
                    if (list.Length == 0)
                    {
                        yield return "Can't find any Client software installation in standard folder";
                    }
                    else
                    {
                        foreach (var item in list)
                        {
                            yield return GetVersionFromFolder(item);
                        }
                    }

               
                }

        }

        private static string GetVersionFromFolder(string folder)
        {
            var assembly = $@"{folder}\bin\Iit.Fibertest.Client.exe";
            if (!File.Exists(assembly))
                return $"Invalid installation in folder {folder}";

            FileVersionInfo vi = FileVersionInfo.GetVersionInfo(assembly);
            return $"Version {vi.FileVersion} in folder {folder}";
        }
    }
}
