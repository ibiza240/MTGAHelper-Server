using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MTGAHelper.Entity.Config.App;
using MTGAHelper.Lib.Config;
using MTGAHelper.Server.Data.Files;
using Newtonsoft.Json;

namespace MTGAHelper.Lib.AllCards.Scryfall
{
    public class ReaderScryfallCards
    {
        //IMapper mapper;

        //public ReaderScryfallCards()
        //{
        //    mapper = new MapperConfiguration(cfg => cfg.AddProfile<MapperProfileLib>()).CreateMapper();
        //}
        private readonly FileLoader fileLoader;

        private readonly string folderData;

        public ReaderScryfallCards(FileLoader fileLoader, IDataPath configFolderData)
        {
            this.fileLoader = fileLoader;
            this.folderData = configFolderData.FolderData;
        }

        public async Task<ICollection<ScryfallModelRootObject>> ReadFileAsync(string fileName)
        {
            var content = await fileLoader.ReadFileContentAsync(Path.Combine(folderData, fileName));
            return Read(content);
        }

        public ICollection<ScryfallModelRootObject> Read(string json)
        {
            var full = JsonConvert.DeserializeObject<ICollection<ScryfallModelRootObject>>(json);

            IEnumerable<ScryfallModelRootObject> data = full;

            //if (onlyArena)
            //    data = data.Where(i => i.arena_id != 0 || i.legalities.standard == "legal" || i.layout == "token" || i.type_line.StartsWith("Basic Land"));

            //var test = data.Where(i => i.name == "Human" && i.set.Contains("trna")).ToArray();
            //var test = data.Where(i => i.type_line.StartsWith("Basic Land")).ToArray();

            var ret = data.ToArray();
            //foreach (var r in ret)
            //    r.set = r.set.ToUpper();

            //var result = mapper.Map<ICollection<Card>>(data);

            //return result;
            return ret;
        }
    }
}