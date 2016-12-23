using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using CommonLogic.Database;
using Fibertest.Datacenter.Web.Domain;
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
        public async Task<int> Put(int id, [FromBody] ClientNode dto)
        {
            _log.Information("change node with {id}", id);
            using (var dbGraphContext = new DbGraphContext(_options))
            {
                var node = dbGraphContext.Nodes.First(n => n.Id == id);
                node.Title = dto.Title;
                node.Latitude = dto.Coors.Latitude;
                node.Longitude = dto.Coors.Longitude;
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