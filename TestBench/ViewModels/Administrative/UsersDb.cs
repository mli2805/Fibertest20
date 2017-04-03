using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Iit.Fibertest.StringResources;
using Serilog;

namespace Iit.Fibertest.TestBench
{
    public class UsersDb
    {
        private readonly ILogger _log;
        private string filename = @"..\db\UsersDb.bin";

        public List<User> Users { get; set; }
        public List<Zone> Zones { get; set; }

        public UsersDb(ILogger log)
        {
            _log = log;
            Load();
            PopulateIfEmpty();
        }

        public void Save()
        {
            try
            {
                using (Stream fStream = new FileStream(filename, FileMode.Create, FileAccess.Write))
                {
                    var binaryFormatter = new BinaryFormatter();

                    binaryFormatter.Serialize(fStream, Users);
                    binaryFormatter.Serialize(fStream, Zones);
                }
                _log.Information(@"Users Db saved successfully.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                _log.Information(e.Message);
            }
        }

        private void Load()
        {
            try
            {
                using (Stream fStream = new FileStream(filename, FileMode.Open, FileAccess.Read))
                {
                    var binaryFormatter = new BinaryFormatter();
                    Users = (List<User>)binaryFormatter.Deserialize(fStream);
                    Zones = (List<Zone>)binaryFormatter.Deserialize(fStream);
                }
                _log.Information(@"Loaded");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                _log.Information(e.Message);
            }

        }

        private void PopulateIfEmpty()
        {
            if (Users == null)
                PopulateUsers();
            if (Zones == null)
                Zones = new List<Zone>()
                {
                    new Zone() {Id = Guid.NewGuid(), Name = Resources.SID_Default_Zone,}
                };
        }

        private void PopulateUsers()
        {
            Users = new List<User>()
            {
                new User()
                {
                    Id = Guid.NewGuid(),
                    Name = @"root",
                    Role = Role.Root,
                    Password = @"root",
                    IsEmailActivated = false,
                    Email = @"main_abonent@yandex.ru",
                    IsDefaultZoneUser = true,
                },
                new User()
                {
                    Id = Guid.NewGuid(),
                    Name = @"operator",
                    Role = Role.Operator,
                    Password = @"operator",
                    IsEmailActivated = true,
                    Email = @"op123op@mail.ru",
                    IsDefaultZoneUser = true,
                },
                new User()
                {
                    Id = Guid.NewGuid(),
                    Name = @"supervisor",
                    Role = Role.Supervisor,
                    Password = @"supervisor",
                    IsEmailActivated = false,
                    Email = "",
                    IsDefaultZoneUser = true,
                },
                new User()
                {
                    Id = Guid.NewGuid(),
                    Name = @"pupkin",
                    Role = Role.Supervisor,
                    Password = @"1234",
                    IsEmailActivated = false,
                    Email = "",
                    IsDefaultZoneUser = false,
                    ZoneId = Guid.Empty
                },
            };
        }
    }
}