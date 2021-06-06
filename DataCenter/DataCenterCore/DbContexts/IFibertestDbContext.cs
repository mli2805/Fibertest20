using System.Data.Entity;

namespace DbExperiments
{
    public interface IFibertestDbContext 
    {
        DbSet<User> Users { get; set; }
        DbSet<MonitoringResult> MonitoringResults { get; set; }

        void SaveChanges();
    }
}