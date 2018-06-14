using Iit.Fibertest.Dto;
using Microsoft.EntityFrameworkCore;

namespace Iit.Fibertest.DatabaseLibrary
{
    public class FtDbContext : DbContext
    {
        public FtDbContext()  { }
        public FtDbContext(DbContextOptions<FtDbContext> options) : base(options) { }


        public DbSet<ClientStation> ClientStations { get; set; }
        public DbSet<RtuStation> RtuStations { get; set; }
        public DbSet<SorFile> SorFiles { get; set; }
    }
}