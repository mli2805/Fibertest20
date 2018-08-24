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

        public Well GetWellByKadastrId(int inKadastrId)
        {
            using (var dbContext = new KadastrDbContext(_kadastrDbSettings.Options))
            {
                return dbContext.Wells.FirstOrDefault(w => w.InKadastrId == inKadastrId);
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
    }
}