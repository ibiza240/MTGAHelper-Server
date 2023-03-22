using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using MTGAHelper.Entity;
using MTGAHelper.Entity.Config.App;
using MTGAHelper.Lib.CardProviders;
using MTGAHelper.Lib.Logging;
using MTGAHelper.Lib.OutputLogParser;
using Newtonsoft.Json;
using Serilog;

namespace MTGAHelper.Lib.Config.Articles
{
    public class ConfigManagerArticles : ConfigManagerBase<ConfigModelArticle>
    {
        protected override string configFileName => "articles.json";

        protected override dynamic GetRoot() => dictValues.Values;

        private readonly CardRepositoryProvider cardRepoProvider;
        private readonly CacheSingleton<IReadOnlyCollection<ConfigModelArticle>> cache;

        private readonly Regex regexCard = new(@"\[\[(.*?)\]\]", RegexOptions.Compiled);

        public ConfigManagerArticles(
            IDataPath configApp,
            CardRepositoryProvider cardRepoProvider
            )
            : base(configApp)
        {
            this.cardRepoProvider = cardRepoProvider;
            this.cache = new CacheSingleton<IReadOnlyCollection<ConfigModelArticle>>(new SimpleLoader<IReadOnlyCollection<ConfigModelArticle>>(LoadData));
        }

        public IReadOnlyCollection<ConfigModelArticle> GetAll()
        {
            return cache.Get();
        }

        public IReadOnlyCollection<ConfigModelArticle> LoadData()
        {
            var p = Path.Combine(this.configApp.FolderData, configFileName);
            //if (File.Exists(p))
            {
                LogExt.LogReadFile(p);
                var fileContent = File.ReadAllText(p);
                var configData = JsonConvert.DeserializeObject<ConfigRootArticles>(fileContent)!;

                foreach (var a in configData.articles)
                {
                    var articleFilePath = Path.Combine(this.configApp.FolderData, "articles", $"{a.Id}.txt");
                    if (File.Exists(articleFilePath))
                    {
                        LogExt.LogReadFile(articleFilePath);
                        var text = File.ReadAllText(articleFilePath);

                        a.Text = regexCard.Replace(text, new MatchEvaluator(GetCardImage));
                    }
                    else
                        Log.Error("File not found: {articleFilePath}", articleFilePath);
                }

                dictValues = configData.articles.ToDictionary(i => i.Id, i => i);

                return Values
                    .OrderByDescending(i => i.DatePosted)
                    .ToArray();
            }
        }

        private string GetCardImage(Match match)
        {
            string cardName = match.Groups[1].Value;
            var card = cardRepoProvider.GetRepository().CardsByName(cardName).FirstOrDefault();

            if (card == null)
                return cardName;
            else
                return $"<span onMouseleave=\"vueApp.showCard(null)\" onMouseOver=\"vueApp.showCard('{card.ImageCardUrl}')\" class=\"cardHover\">{cardName}</span>";
        }
    }
}