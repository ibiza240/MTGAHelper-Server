//using Microsoft.Extensions.Options;
//using MTGAHelper.Entity;
//using MTGAHelper.Lib.Cache;
//using MTGAHelper.Lib.Config;
//using MTGAHelper.Lib.Logging;
//using Newtonsoft.Json;
//using Serilog;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;

//namespace MTGAHelper.Lib.IO.Reader
//{
//    public class AllDecksLoader
//    {
//        ConfigModelApp configApp;
//        ConfigManagerDecks configDecks;
//        CacheSingleton<IReadOnlyCollection<ConfigModelDeck>> cache;
//        CacheSingleton<IReadOnlyCollection<Card>> cacheCards;

//        public AllDecksLoader(IOptionsMonitor<ConfigModelApp> configApp,
//            ConfigManagerDecks configDecks,
//            CacheSingleton<IReadOnlyCollection<ConfigModelDeck>> cache,
//            CacheSingleton<IReadOnlyCollection<Card>> cacheCards)
//        {
//            this.configApp = configApp.CurrentValue;
//            this.configDecks = configDecks;
//            this.cache = cache;
//            this.cacheCards = cacheCards;

//            cache.PopulateIfNotSet(LoadDecks);
//        }

//        ICollection<IDeck> LoadDecks()
//        {
//            var allCards = cacheCards.Get();

//            var p = Path.Combine(this.configApp.FolderData, "decks.json");
//            if (File.Exists(p))
//            {
//                Log.Information("{nbDecks} decks to load", configDecks.Values.Count);

//                LogExt.LogReadFile(p);
//                var fileContent = File.ReadAllText(p);
//                var jsonResult = JsonConvert.DeserializeObject<ConfigRootDecks>(fileContent).decks;
//                var dictValues = jsonResult.GroupBy(i => i.Id).ToDictionary(i => i.Key, i => i.Last()).Select(i => i.Value).ToArray();

//                foreach (var d in dictValues)
//                {
//                    var test = d.CardsMain.Where(i => allCards.Any(x => x.grpId == i.Key) == false).ToArray();
//                    if (test.Length > 0)
//                        System.Diagnostics.Debugger.Break();

//                    var cardsMain = d.CardsMain.Select(i => new DeckCard(
//                        new CardWithAmount(allCards.First(x => x.grpId == i.Key), i.Value), false));
//                    var cardsSideboard = d.CardsSideboard.Select(i => new DeckCard(
//                        new CardWithAmount(allCards.First(x => x.grpId == i.Key), i.Value), true));

//                    d.Deck = new Deck(d.Name, new ScraperType(d.ScraperTypeId), cardsMain.Union(cardsSideboard).ToArray());
//                }

//                Log.Information("{nbDecks} decks loaded", configDecks.Values.Where(i => i.Deck != null).Count());
//                return dictValues.Select(i => i.Deck).ToArray();
//            }

//            return new IDeck[0];
//        }

//        public void ReloadDecks()
//        {
//            cache.Set(LoadDecks());
//        }
//    }
//}