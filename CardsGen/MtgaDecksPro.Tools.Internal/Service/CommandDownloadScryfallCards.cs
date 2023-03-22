using HtmlAgilityPack;
using Minmaxdev.Cache.Common.Service;
using MtgaDecksPro.Cards.BootstrapCardsBuilding.Entity.Scryfall;
using MtgaDecksPro.Cards.Entity.Config;
using Serilog;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace MtgaDecksPro.Tools.Internal.Service
{
    public class CommandDownloadScryfallCards : ICommand
    {
        private readonly ILogger logger;
        private readonly ICacheHandler<List<ScryfallModelRootObjectExtended>> cacheHandlerScryfallCard;
        private readonly IHttpClientFactory httpClientFactory;
        private string folderData;

        public CommandDownloadScryfallCards(
            ILogger logger,
            IConfigFolderDataCards configFolderDataCards,
            ICacheHandler<List<ScryfallModelRootObjectExtended>> cacheHandlerScryfallCard,
            IHttpClientFactory httpClientFactory
            )
        {
            this.folderData = configFolderDataCards.FolderDataCards;
            this.logger = logger;
            this.cacheHandlerScryfallCard = cacheHandlerScryfallCard;
            this.httpClientFactory = httpClientFactory;
        }

        public async Task Execute()
        {
            logger.Information("Downloading Scryfall Default Cards...");

            HtmlWeb hw = new HtmlWeb();
            HtmlDocument doc = await hw.LoadFromWebAsync("https://scryfall.com/docs/api/bulk-data");
            var links = doc.DocumentNode.SelectNodes("//table//a").Select(i => i.Attributes["href"].Value);

            await DownloadCards(links);
            await DownloadSets();
        }

        private async Task DownloadCards(IEnumerable<string> links)
        {
            var urlDefaultCards = links.First(i => i.Contains("default-cards"));
            var localFile = Path.GetFullPath(Path.Combine(folderData, "cardsbuilder", "scryfall-default-cards.json"));
            await DownloadToFile(urlDefaultCards, localFile);
        }

        private async Task DownloadSets()
        {
            var url = "https://api.scryfall.com/sets";
            var localFile = Path.GetFullPath(Path.Combine(folderData, "cardsbuilder", "scryfall-sets.json"));
            await DownloadToFile(url, localFile);
        }

        private async Task DownloadToFile(string url, string toFile)
        {
            using (var client = httpClientFactory.CreateClient())
            {
                var response = await client.GetAsync(url);

                using (var fs = new FileStream(toFile, FileMode.Create))
                {
                    await response.Content.CopyToAsync(fs);
                }

                await cacheHandlerScryfallCard.ForceExpire();
                await cacheHandlerScryfallCard.Get();
            }
        }
    }
}