using System.Data.Entity;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.DatabaseLibrary.DbContexts
{
    public interface IFibertestDbContext 
    {
        DbSet<User> Users { get; set; }

        void SaveChanges();
    }
}