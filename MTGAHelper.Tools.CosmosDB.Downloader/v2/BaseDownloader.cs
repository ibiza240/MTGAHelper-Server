using MTGAHelper.Server.Data.CosmosDB;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MTGAHelper.Tools.CosmosDB.Downloader.v2
{
    public class BaseDownloader
    {
        protected readonly UserDataCosmosManager userDataCosmosManager;

        public BaseDownloader(
            UserDataCosmosManager userDataCosmosManager
            )
        {
            this.userDataCosmosManager = userDataCosmosManager;
        }

        protected ICollection<string> GetUserIds(string folder)
        {
            var userIds = Directory.GetDirectories(folder)
                .Select(i => Path.GetFileName(i))
                //.Select(i => (i, JsonConvert.DeserializeObject<ConfigModelUser>(File.ReadAllText(Path.Join(folder, $"{i}_userconfig.json"))).LastLoginUtc))
                //.Where(i => i.Contains("21934bf12e904cd48bca78a3316e547a"))
                //.Take(5)
                .ToArray();

            return userIds;
        }
    }
}