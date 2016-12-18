using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Logic.Database;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Fibertest.Datacenter.Web.Controllers
{
    /// <summary>1 </summary>
    public class GraphController : ApiController
    {
        private readonly DbContextOptions<GraphContext> _options;
        private readonly ILogger _log;

        /// <summary>2 </summary>
        public GraphController(DbContextOptions<GraphContext> options, ILogger log)
        {
            _options = options;
            _log = log;

        }

        /// <summary>13 </summary>
        [Route("api/graph/node")]
        public async Task<int> Post([FromBody] CoordinatesArg arg)
        {
            _log.Information("create node {coordinates}", arg);
            using (var ctx = new GraphContext(_options))
            {
                var node = new Node { Latitude = arg.Latitude, Longitude = arg.Longitude };
                ctx.Add(node);
                await ctx.SaveChangesAsync();
                return node.Id;
            }
        }

        /// <summary>3 </summary>
        [Route("api/graph")]
        public async Task<object> Get()
        {
            using (var ctx = new GraphContext(_options))
            {
                var nodes = await ctx.Nodes.ToListAsync();
                return new
                {
                    Nodes = nodes.Select(n => new
                        {
                            Coordinates = new
                            {
                                n.Latitude,
                                n.Longitude
                            }
                        }
                    )
                };
            }
        }


    }
}