
using Microsoft.Web.Administration;
using System.DirectoryServices;
using System.Linq;

namespace Iit.Fibertest.UtilsLib
{
    public static class IisOperations
    {
        public static bool DoesWebsiteExist(string websiteName)
        {
            var w3svc = new DirectoryEntry("IIS://localhost/w3svc",
                "username", "password", AuthenticationTypes.None);

            foreach (DirectoryEntry site in w3svc.Children)
            {
                if (site.Properties["ServerComment"]?.Value == null)
                    continue;
                if (site.Properties["ServerComment"].Value.ToString() == websiteName)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// creates and at once starts web site
        /// </summary>
        /// <param name="websiteName">fibertest_angular_app</param>
        /// <param name="bindingProtocol">http</param>
        /// <param name="bindingInformation">*:8080:</param>
        /// <param name="path">d:\\MySite</param>
        public static void CreateWebsite(string websiteName, string bindingProtocol, string bindingInformation,
            string path)
        {
            ServerManager iisManager = new ServerManager();
            iisManager.Sites.Add(websiteName, bindingProtocol, bindingInformation, path);
            iisManager.CommitChanges();
        }

        public static bool StopWebsite(string websiteName)
        {
            var iisManager = new ServerManager();
            var site = iisManager.Sites.FirstOrDefault(s => s.Name == websiteName);
            if (site == null) return true;
            site.Stop();
            return site.State == ObjectState.Stopped;
        }

        public static bool StartWebsite(string websiteName)
        {
            var iisManager = new ServerManager();
            var site = iisManager.Sites.FirstOrDefault(s => s.Name == websiteName);
            if (site == null) return true;
            site.Start();
            return site.State == ObjectState.Started;
        }
    }
}