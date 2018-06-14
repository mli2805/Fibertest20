using Microsoft.EntityFrameworkCore;

namespace Iit.Fibertest.DatabaseLibrary
{
    public interface ISettings
    {
        void Init();
        DbContextOptions<FtDbContext> Options { get; }

    }
}