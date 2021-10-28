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

        public static ClientStation GetClientByConnectionId(this ClientsCollection collection, string connectionId)
        {
            return connectionId == null ? null : collection.Clients.FirstOrDefault(c => c.ConnectionId == connectionId);
        }

        public static bool HasAnyWebClients(this ClientsCollection collection)
        {
            return collection.Clients.Any(s => s.IsWebClient);
        }

        public static ClientStation GetClientByClientIp(this ClientsCollection collection, string clientIp)
        {
            return collection.Clients.FirstOrDefault(c => c.ClientIp == clientIp);
        }

        // public DoubleAddress GetOneDesktopClientAddress(string clientIp)
        // {
        //     if (clientIp == null)
        //         return null;
        //     var client = _clients.FirstOrDefault(c => c.ClientIp == clientIp && !c.IsWebClient);
        //     return client == null
        //         ? null
        //         : new DoubleAddress() { Main = new NetAddress(client.ClientIp, client.ClientAddressPort) };
        // }
        //
        // public DoubleAddress GetClientAddressByConnectionId(string connectionId)
        // {
        //     if (connectionId == null)
        //         return null;
        //
        //     var client = _clients.FirstOrDefault(c => c.ConnectionId == connectionId);
        //     return client == null
        //         ? null
        //         : new DoubleAddress() { Main = new NetAddress(client.ClientIp, client.ClientAddressPort) };
        // }

      
        // public List<string> GetWebClientsId()
        // {
        //     return _clients.Where(c => c.IsWebClient).Select(l => l.ConnectionId).ToList();
        // }
        //
        // public List<ClientStation> GetWebClients()
        // {
        //     return _clients.Where(c => c.IsWebClient).ToList();
        // }

        // public ClientStation GetStationByConnectionId(string connectionId)
        // {
        //     return _clients.FirstOrDefault(s => s.ConnectionId == connectionId);
        // }


    }
}
