using Microsoft.EntityFrameworkCore;

namespace CommonLogic.Database
{
    public class DbGraphContext : DbContext
    {
        public DbGraphContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<DtoNode> Nodes { get; set; }
    }
}