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
            new ApplyLicense()
            {
                Owner = "Demo license",
                RtuCount = new LicenseParameter() { Value = 1, ValidUntil = DateTime.MaxValue },
                ClientStationCount = new LicenseParameter() { Value = 2, ValidUntil = DateTime.MaxValue },
                WebClientCount = new LicenseParameter() {Value = 1, ValidUntil = DateTime.MaxValue },
                SuperClientStationCount = new LicenseParameter() { Value = 1, ValidUntil = DateTime.Today.AddMonths(6) },
                Version = "2.5.0.1"
            },
            new AddZone() { IsDefaultZone = true, Title = StringResources.Resources.SID_Default_Zone },
            new AddUser() { UserId = Guid.NewGuid(), Title = "developer",
                EncodedPassword = UserExt.FlipFlop("developer"), Role = Role.Developer, ZoneId = Guid.Empty },
            new AddUser() { UserId = Guid.NewGuid(), Title = "root",
                EncodedPassword = UserExt.FlipFlop("root"), Role = Role.Root, ZoneId = Guid.Empty },
            new AddUser() { UserId = Guid.NewGuid(), Title = "operator", EncodedPassword = UserExt.FlipFlop("operator"),
                Role = Role.Operator, ZoneId = Guid.Empty },
            new AddUser() { UserId = Guid.NewGuid(), Title = "supervisor", EncodedPassword = UserExt.FlipFlop("supervisor"),
                Role = Role.Supervisor, ZoneId = Guid.Empty },
            new AddUser() { UserId = Guid.NewGuid(), Title = "weboperator", EncodedPassword = UserExt.FlipFlop("weboperator"),
                Role = Role.WebOperator, ZoneId = Guid.Empty },
            new AddUser() { UserId = Guid.NewGuid(), Title = "websupervisor", EncodedPassword = UserExt.FlipFlop("websupervisor"),
                Role = Role.WebSupervisor, ZoneId = Guid.Empty },
            new AddUser() { UserId = Guid.NewGuid(), Title = "superclient", EncodedPassword = UserExt.FlipFlop("superclient"),
                Role = Role.Superclient, ZoneId = Guid.Empty },
        };
    }
}