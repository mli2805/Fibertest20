using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.SuperClient
{
    public class ClientForSuper
    {
        public string Version;
        public string Path;
    }

    public class ClientList
    {
        public bool IsSuccess;
        public string ErrorMessage;
        public List<ClientForSuper> Clients;

        public ClientList(string errorMessage)
        {
            ErrorMessage = errorMessage;
        }

        public ClientList(List<ClientForSuper> clients)
        {
            IsSuccess = true;
            Clients = clients;
        }
    }

    public static class Utils
    {
        public static List<string> ListForView(this ClientList clientList)
        {
            if (clientList.IsSuccess)
                return clientList.Clients.Select(Map).ToList();
            return new List<string>() { clientList.ErrorMessage };
        }

        public static ClientList GetClients()
        {
            try
            {
                var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                if (assemblyPath == null)
                    return new ClientList(Resources.SID_Can_t_get_current_assembly_path);

#if DEBUG
                assemblyPath = @"c:\Iit-Fibertest\SuperClient\bin\";
#endif

                while (!assemblyPath.EndsWith(@"SuperClient"))
                {
                    assemblyPath = FileOperations.GetParentFolder(assemblyPath);
                }

                var basePath = FileOperations.GetParentFolder(assemblyPath) + @"\";

                string pattern = @"client.*";

                if (!Directory.Exists(basePath))
                    return new ClientList(string.Format(Resources.SID_Can_t_find_base_folder__0_, basePath));

                var list = Directory.GetDirectories(basePath, pattern);
                if (list.Length == 0)
                    return new ClientList(string.Format(Resources.SID_Can_t_find_any_Client_software_installation_in__0_, basePath));

                return new ClientList(list.Select(GetVersionFromFolder).ToList());
            }
            catch (Exception e)
            {
                return new ClientList(e.Message + Environment.NewLine + e.InnerException?.Message);
            }
        }

        private static string Map(ClientForSuper cl)
        {
            return cl.Version == null
                ? $"Invalid installation in folder {cl.Path}"
                : $"Version {cl.Version} in folder {cl.Path}";
        }

        private static ClientForSuper GetVersionFromFolder(string folder)
        {
            var assembly = $@"{folder}\bin\Iit.Fibertest.Client.exe";
            if (!File.Exists(assembly))
                return new ClientForSuper() { Path = folder, Version = null };

            FileVersionInfo vi = FileVersionInfo.GetVersionInfo(assembly);
            return new ClientForSuper() { Path = folder, Version = vi.FileVersion };
        }
    }
}
