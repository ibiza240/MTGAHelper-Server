using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using MTGAHelper.Entity.Config.App;
using MTGAHelper.Server.Data.Files;
using Newtonsoft.Json;

namespace MTGAHelper.Server.DataAccess.CacheUserHistory
{
    public class CacheDataGlobal<T> where T : new()
    {
        private readonly string folderDataGlobal;

        private ConcurrentDictionary<string, Task<T>> Data { get; } = new ConcurrentDictionary<string, Task<T>>();
        private readonly FileLoader fileLoader;

        public CacheDataGlobal(FileLoader fileLoader, IDataPath configPath)
        {
            this.fileLoader = fileLoader;
            this.folderDataGlobal = Path.Combine(configPath.FolderData, "global");
        }

        public void Invalidate(string filePathRelativeToGlobal)
        {
            Data.TryRemove(filePathRelativeToGlobal, out _);
        }

        public Task<T> GetData(string filePathRelativeToGlobal)
        {
            return Data.GetOrAdd(filePathRelativeToGlobal, LoadDataFromDisk);
        }

        private async Task<T> LoadDataFromDisk(string filePathRelativeToGlobal)
        {
            if (filePathRelativeToGlobal == null)
                return default;

            var filePath = Path.Combine(folderDataGlobal, filePathRelativeToGlobal);
            var json = await fileLoader.ReadFileContentAsync(filePath, "global");
            return json == null ? default : JsonConvert.DeserializeObject<T>(json);
        }
    }
}