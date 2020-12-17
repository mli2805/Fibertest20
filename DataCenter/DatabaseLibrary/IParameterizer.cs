using Microsoft.EntityFrameworkCore;

namespace Iit.Fibertest.DatabaseLibrary
{
    public interface IParameterizer
    {
        void Init();
        void LogSettings();
        DbContextOptions<FtDbContext> Options { get; }

    }
}