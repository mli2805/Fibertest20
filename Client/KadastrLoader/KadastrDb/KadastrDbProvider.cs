using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KadastrLoader
{
    public class KadastrDbProvider
    {
        private readonly KadastrDbSettings _kadastrDbSettings;

        public KadastrDbProvider(KadastrDbSettings kadastrDbSettings)
        {
            _kadastrDbSettings = kadastrDbSettings;
        }

        public List<Well> GetWells()
        {
            using (var dbContext = new KadastrDbContext(_kadastrDbSettings.Options))
            {
                return dbContext.Wells.ToList();
            }
        }

        public async Task<int> AddWell(Well well)
        {
            using (var dbContext = new KadastrDbContext(_kadastrDbSettings.Options))
            {
                dbContext.Wells.Add(well);
                return await dbContext.SaveChangesAsync();
            }
        }

        public async Task<int> AddConpoint(Conpoint conpoint)
        {
            using (var dbContext = new KadastrDbContext(_kadastrDbSettings.Options))
            {
                dbContext.Conpoints.Add(conpoint);
                return await dbContext.SaveChangesAsync();
            }
        }
    }
}