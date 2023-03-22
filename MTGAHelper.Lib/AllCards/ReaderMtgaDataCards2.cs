using MTGAHelper.Entity.Config.App;
using MTGAHelper.Lib.AllCards.MtgaDataCards;
using MTGAHelper.Lib.AllCards.MtgaDataLoc;
using MTGAHelper.Server.Data.Files;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MTGAHelper.Lib.AllCards
{
    public class ReaderMtgaDataCards2
    {
        private readonly string folderData;
        private readonly FileLoader fileLoader;

        public ReaderMtgaDataCards2(FileLoader fileLoader, IDataPath folderDataPath)
        {
            folderData = folderDataPath.FolderData;
            this.fileLoader = fileLoader;
        }

        public async Task<ICollection<MtgaDataCardsRootObjectExtended>> GetMtgaCardsAsync()
        {
            var mtgaCardsStr = await fileLoader.ReadFileContentAsync(Path.Combine(folderData, "data_cards.mtga"));
            var mtgaCards = JsonConvert.DeserializeObject<ICollection<MtgaDataCardsRootObjectExtended>>(mtgaCardsStr);

            var mtgaCardsLocStr = await fileLoader.ReadFileContentAsync(Path.Combine(folderData, "data_loc.mtga"));
            var mtgaLoc = JsonConvert.DeserializeObject<ICollection<MtgaDataLocRootObject>>(mtgaCardsLocStr);

            var dictLoc = mtgaLoc.First(i => i.isoCode == "en-US").keys.ToDictionary(i => i.id, i => i.text);

            foreach (var c in mtgaCards)
            {
                if (dictLoc.ContainsKey(c.titleId))
                    c.Name = dictLoc[c.titleId];
                else
                {
                    Debug.WriteLine($"Cannot find titleId {c.titleId} in loc file");
                }
            }

            return mtgaCards;
        }
    }
}