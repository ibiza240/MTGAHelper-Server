using System.Collections.Generic;
using System.IO;
using System.Linq;
using MTGAHelper.Entity.Config.App;
using MTGAHelper.Lib.Logging;
using Newtonsoft.Json;
using System;

namespace MTGAHelper.Lib.Config.News
{
    public class ConfigManagerNews : ConfigManagerBase<ConfigModelNews>, ICacheLoader<IReadOnlyCollection<ConfigModelNews>>
    {
        protected override string configFileName => "news.json";

        protected override dynamic GetRoot() => new { news = dictValues.Values, ignored };

        public ICollection<string> ignored { get; set; } = Array.Empty<string>();

        public ConfigManagerNews(IDataPath configApp)
            : base(configApp) { }

        public IReadOnlyCollection<ConfigModelNews> LoadData()
        {
            var p = Path.Combine(this.configApp.FolderData, configFileName);
            if (File.Exists(p))
            {
                LogExt.LogReadFile(p);
                var fileContent = File.ReadAllText(p);
                var configData = JsonConvert.DeserializeObject<ConfigRootNews>(fileContent);
                dictValues = configData.news.ToDictionary(i => i.Id, i => i);
                ignored = configData.ignored ?? Array.Empty<string>();
            }

            return Values
                .Where(i => ignored.Contains(i.Id) == false)
                .OrderByDescending(i => i.DatePosted)
                .ToArray();
        }

        public void RefreshCache(CacheSingleton<IReadOnlyCollection<ConfigModelNews>> cache)
        {
            cache.Reload();
        }
    }
}