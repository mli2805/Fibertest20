using System.Data.Entity;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.DatabaseLibrary.DbContexts
{
    public interface IFibertestDbContext
    {
        DbSet<User> Users { get; set; }
        DbSet<ClientStation> ClientStations { get; set; }
        DbSet<RtuStation> RtuStations { get; set; }

        void SaveChanges();
        Task<int> SaveChangesAsync();
    }
}