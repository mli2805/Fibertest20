using System.Collections.Generic;
using System.Linq;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.DataCenterCore
{
    public static class ClientsInCollectionFinder
    {
        public static List<DoubleAddress> GetAllDesktopClientsAddresses(this ClientsCollection collection)
        {
            return collection.Clients
                .Where(s => !s.IsWebClient)
                .Select(c => new DoubleAddress() { Main = new NetAddress(c.ClientIp, c.ClientAddressPort) })
                .ToList();
        }

        public static ClientStation Get(this ClientsCollection collection, string connectionId)
        {
            if (connectionId == null) return null;
            if (connectionId == collection.TrapConnectionId) return new ClientStation() { UserName = collection.ServerNameForTraps };

            return collection.Clients.FirstOrDefault(c => c.ConnectionId == connectionId);
        }

        public static bool HasAnyWebClients(this ClientsCollection collection)
        {
            return collection.Clients.Any(s => s.IsWebClient);
        }
    }
}
