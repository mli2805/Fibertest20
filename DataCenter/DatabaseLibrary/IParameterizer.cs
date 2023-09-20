using Microsoft.EntityFrameworkCore;

namespace Iit.Fibertest.DatabaseLibrary
{
    public interface IParameterizer
    {
        void Init();
        void LogSettings();
        string MySqlConnectionString { get; }
        DbContextOptions<FtDbContext> Options { get; }

    }
}