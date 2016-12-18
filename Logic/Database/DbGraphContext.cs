using Microsoft.EntityFrameworkCore;

namespace Logic.Database
{
    public class DbGraphContext : DbContext
    {
        public DbGraphContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<DbNode> Nodes { get; set; }
    }
}