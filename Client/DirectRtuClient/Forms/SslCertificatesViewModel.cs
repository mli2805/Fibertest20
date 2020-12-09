using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Caliburn.Micro;

namespace DirectRtuClient
{
    public class SslCertificatesViewModel : Screen
    {
        public List<string> Certificates { get; set; }

        public SslCertificatesViewModel()
        {
            Certificates = GetCertificates().ToList();
        }

        private IEnumerable<string> GetCertificates()
        {
            X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            store.Open(OpenFlags.OpenExistingOnly | OpenFlags.ReadWrite);

            foreach (var cert in store.Certificates)
                yield return cert.FriendlyName;
            store.Close();
        }
    }
}
