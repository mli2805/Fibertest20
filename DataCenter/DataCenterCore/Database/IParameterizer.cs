using Microsoft.EntityFrameworkCore;

namespace Iit.Fibertest.DataCenterCore
{
    public interface IParameterizer
    {
        void Init();
        void LogSettings();
        string MySqlConnectionString { get; }
        DbContextOptions<FtDbContext> Options { get; }

    }
}