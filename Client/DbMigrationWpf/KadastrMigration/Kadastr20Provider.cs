using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Iit.Fibertest.Graph;
using Microsoft.EntityFrameworkCore;

namespace DbMigrationWpf
{
    public class KadastrDbContext : DbContext
    {
        public KadastrDbContext(DbContextOptions<KadastrDbContext> options) : base(options) { }

        public DbSet<Well> Wells { get; set; }
        public DbSet<Conpoint> Conpoints { get; set; }
    }
    
    public class Kadastr20Provider
    {
        private readonly string _mysqlServerAddress;
        private readonly int _mysqlTcpPort;
        private readonly ObservableCollection<string> _progressLines;

        private string MySqlConnectionString => $"server={_mysqlServerAddress};port={_mysqlTcpPort};user id=root;password=root;database=ft20kadastr";
        public DbContextOptions<KadastrDbContext> Options =>
            new DbContextOptionsBuilder<KadastrDbContext>().UseMySql(MySqlConnectionString).Options;     

        public Kadastr20Provider(string serverIp, int mysqlTcpPort, ObservableCollection<string> progressLines)
        {
            _mysqlServerAddress = serverIp;
            _mysqlTcpPort = mysqlTcpPort;
            _progressLines = progressLines;
        }

        public void Init()
        {
            using (var dbContext = new KadastrDbContext(Options))
            {
                dbContext.Database.EnsureCreated();
            }
        }

        public async Task<int> Save(KadastrModel model)
        {
            using (var dbContext = new KadastrDbContext(Options))
            {
                dbContext.Wells.AddRange(model.Wells);
                dbContext.Conpoints.AddRange(model.Conpoints);
                var count = await dbContext.SaveChangesAsync();
                _progressLines.Add($"{count} records saved");
                return count;
            }
        }

    }
}