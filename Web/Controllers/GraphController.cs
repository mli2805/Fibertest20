using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Fibertest.Datacenter.Web.Domain;
using LogicOnServer.Database;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Fibertest.Datacenter.Web.Controllers
{
    /// <summary>1 </summary>
    public class GraphController : ApiController
    {
        private readonly DbContextOptions<DbGraphContext> _options;
        private readonly ILogger _log;

        /// <summary>CRUD requests to graph</summary>
        public GraphController(DbContextOptions<DbGraphContext> options, ILogger log)
        {
            _options = options;
            _log = log;

        }

        /// <summary>Put used to change existing entity</summary>
        [Route("api/graph/node/id")]
        public async Task<int> Put(int id, [FromBody] NodeArg arg)
        {
            _log.Information("change node with {id}", id);
            using (var dbGraphContext = new DbGraphContext(_options))
            {
                var node = dbGraphContext.Nodes.First(n => n.Id == id);
                node.Title = arg.Title;
                node.Latitude = arg.Coors.Latitude;
                node.Longitude = arg.Coors.Longitude;
                await dbGraphContext.SaveChangesAsync();
                return 200;
            }
        }

        /// <summary>Post used for create entities</summary>
        [Route("api/graph/node")]
        public async Task<int> Post([FromBody] CoordinatesArg arg)
        {
            _log.Information("create node {coordinates}", arg);
            using (var dbGraphContext = new DbGraphContext(_options))
            {
                var node = new DbNode { Latitude = arg.Latitude, Longitude = arg.Longitude };
                dbGraphContext.Add(node);
                await dbGraphContext.SaveChangesAsync();
                return node.Id;
            }
        }

        /// <summary>Get used to read entities</summary>
        [Route("api/graph")]
        public async Task<object> Get()
        {
            using (var dbGraphContext = new DbGraphContext(_options))
            {
                var nodes = await dbGraphContext.Nodes.ToListAsync();
                return new { Nodes = nodes.Select(n => new
                                 { Coordinates = new { n.Latitude, n.Longitude } } ) };
            }
        }



    }
}