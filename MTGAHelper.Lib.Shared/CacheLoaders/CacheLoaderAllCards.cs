using System.Collections.Generic;
using System.IO;
using System.Linq;
using MTGAHelper.Entity;
using MTGAHelper.Entity.Config.App;
using MTGAHelper.Lib.Logging;
using Newtonsoft.Json;

namespace MTGAHelper.Lib.CacheLoaders
{
    public class CacheLoaderAllCards : ICacheLoader<IReadOnlyDictionary<int, Card>>
    {
        readonly string folderData;

        public CacheLoaderAllCards(IDataPath config)
        {
            folderData = config.FolderData;
        }

        public IReadOnlyDictionary<int, Card> LoadData()
        {
            var fileSets = Path.Combine(folderData, "AllCardsCached2.json");
            LogExt.LogReadFile(fileSets);
            var content = File.ReadAllText(fileSets);
            var dataOldFormat = JsonConvert.DeserializeObject<ICollection<Card2>>(content)!;

            // Patch to support multiple cards with an Arena Id of 0
            var iNewUniqueId = 0;
            foreach (Card2 cardMissingId in dataOldFormat.Where(i => i.IdArena == 0))
            {
                cardMissingId.IdArena = iNewUniqueId--;
            }

            return dataOldFormat.Select(c => new Card(c)).ToDictionary(i => i.GrpId);
        }
    }
}