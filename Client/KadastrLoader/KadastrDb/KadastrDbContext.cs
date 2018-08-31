using Iit.Fibertest.Graph;
using Microsoft.EntityFrameworkCore;

namespace KadastrLoader
{
    public class KadastrDbContext : DbContext
    {
        public KadastrDbContext(DbContextOptions<KadastrDbContext> options) : base(options) { }

        public DbSet<Well> Wells { get; set; }
        public DbSet<Conpoint> Conpoints { get; set; }
    }
}
