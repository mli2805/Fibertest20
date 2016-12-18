using Microsoft.EntityFrameworkCore;

namespace LogicOnServer.Database
{
    public class DbGraphContext : DbContext
    {
        public DbGraphContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<DbNode> Nodes { get; set; }
    }
}