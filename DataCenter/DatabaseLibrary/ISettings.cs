using Microsoft.EntityFrameworkCore;

namespace Iit.Fibertest.DatabaseLibrary
{
    public interface ISettings
    {
        void Init();
        void LogSettings();
        DbContextOptions<FtDbContext> Options { get; }

    }
}