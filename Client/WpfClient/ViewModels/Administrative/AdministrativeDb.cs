using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;
using Serilog;

namespace Iit.Fibertest.Client
{
    public class AdministrativeDb
    {
        private readonly IMyLog _logFile;
        private string filename = @"..\db\AdministrativeDb.bin";

        public List<User> Users { get; set; }
        public List<Zone> Zones { get; set; }

        public AdministrativeDb(IMyLog logFile)
        {
            _logFile = logFile;
            Load();
            PopulateIfEmpty();
        }

        public bool CheckPassword(string userName, string password)
        {
            var user = Users.FirstOrDefault(u => u.Name == userName);
            if (user == null)
                return false;
            return user.Password == password;
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
                _logFile.AppendLine(@"Administrative Db saved successfully.");
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
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
                _logFile.AppendLine(@"Loaded");
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
            }

        }

        private void PopulateIfEmpty()
        {
            if (Users == null)
                PopulateUsers();
            if (Zones == null)
                Zones = new List<Zone>()
                {
                    new Zone() {Id = Guid.NewGuid(), Title = Resources.SID_Default_Zone,}
                };
            Save();
        }

        private void PopulateUsers()
        {
            Users = new List<User>()
            {
                new User()
                {
                    Id = Guid.NewGuid(),
                    Name = @"developer",
                    Role = Role.Developer,
                    Password = @"developer",
                    IsEmailActivated = false,
                    Email = "",
                    IsDefaultZoneUser = true,
                },
                new User()
                {
                    Id = Guid.NewGuid(),
                    Name = @"root",
                    Role = Role.Root,
                    Password = @"root",
                    IsEmailActivated = false,
                    Email = "",
                    IsDefaultZoneUser = true,
                },
                new User()
                {
                    Id = Guid.NewGuid(),
                    Name = @"operator",
                    Role = Role.Operator,
                    Password = @"operator",
                    IsEmailActivated = false,
                    Email = "",
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
            };
        }
    }
}