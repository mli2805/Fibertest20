using System.Data.Entity;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.DbLibrary.DbContexts
{
    public interface IFibertestDbContext 
    {
        DbSet<User> Users { get; set; }

        void SaveChanges();
    }
}