using System;
using System.Collections.Generic;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.DataCenterCore
{
    public static class DbSeeds
    {
        public static readonly List<object> Collection = new List<object>()
        {
            new AddZone() { IsDefaultZone = true, Title = StringResources.Resources.SID_Default_Zone },
            new AddUser() { UserId = Guid.NewGuid(), Title = "developer",
                EncodedPassword = "developer".GetHashString(), Role = Role.Developer, ZoneId = Guid.Empty },
            new AddUser() { UserId = Guid.NewGuid(), Title = "root",
                EncodedPassword = "root".GetHashString(), Role = Role.Root, ZoneId = Guid.Empty },
            new AddUser() { UserId = Guid.NewGuid(), Title = "operator", 
                EncodedPassword = "operator".GetHashString(), Role = Role.Operator, ZoneId = Guid.Empty },
            new AddUser() { UserId = Guid.NewGuid(), Title = "supervisor", 
                EncodedPassword = "supervisor".GetHashString(), Role = Role.Supervisor, ZoneId = Guid.Empty },
            new AddUser() { UserId = Guid.NewGuid(), Title = "weboperator", 
                EncodedPassword = "weboperator".GetHashString(), Role = Role.WebOperator, ZoneId = Guid.Empty },
            new AddUser() { UserId = Guid.NewGuid(), Title = "websupervisor", 
                EncodedPassword = "websupervisor".GetHashString(), Role = Role.WebSupervisor, ZoneId = Guid.Empty },
            new AddUser() { UserId = Guid.NewGuid(), Title = "superclient", 
                EncodedPassword = "superclient".GetHashString(), Role = Role.SuperClient, ZoneId = Guid.Empty },
        };
    }
}