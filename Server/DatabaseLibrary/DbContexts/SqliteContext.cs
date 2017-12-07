﻿using System.Data.Entity;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.DatabaseLibrary.DbContexts
{
    public class SqliteContext : DbContext, IFibertestDbContext
    {
        public SqliteContext() : base("sqlitedb") { }

        public DbSet<User> Users { get; set; }
        public DbSet<ClientStation> ClientStations { get; set; }
        public DbSet<RtuStation> RtuStations { get; set; }
        public DbSet<BaseRef> BaseRefs { get; set; }
        public DbSet<NetworkEvent> NetworkEvents { get; set; }
        public DbSet<Measurement> Measurements { get; set; }
        public DbSet<SorFile> SorFiles { get; set; }

        public new void SaveChanges()
        {
            base.SaveChanges();
        }
        public new async Task<int> SaveChangesAsync()
        {
            return await base.SaveChangesAsync();
        }
    }
}
