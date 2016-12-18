using Microsoft.EntityFrameworkCore;

namespace Logic.Database
{
    public class GraphContext : DbContext
    {
        public GraphContext(DbContextOptions options) : base(options)
        {
        }

      

        public DbSet<Node> Nodes { get; set; }
    }
}