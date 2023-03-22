using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MTGAHelper.Entity;
using MTGAHelper.Entity.Config.App;
using MTGAHelper.Entity.Config.Decks;
using MTGAHelper.Lib.CardProviders;
using MTGAHelper.Lib.Logging;
using Newtonsoft.Json;
using Serilog;

namespace MTGAHelper.Lib.Config.Decks
{
    public class ConfigManagerDecks
    {
        private readonly string path;
        private readonly CardRepositoryProvider cardRepoProvider;
        private readonly CacheSingleton<IReadOnlyCollection<ConfigModelDeck>> cache;

        public ConfigManagerDecks(ConfigModelApp configApp,
            CardRepositoryProvider cardRepoProvider)
        {
            path = Path.Combine(configApp.FolderData, "decks.json");
            this.cardRepoProvider = cardRepoProvider;
            cache = new CacheSingleton<IReadOnlyCollection<ConfigModelDeck>>(new SimpleLoader<IReadOnlyCollection<ConfigModelDeck>>(LoadData));
        }

        private IReadOnlyCollection<ConfigModelDeck> LoadData()
        {
            var dictCards = cardRepoProvider.GetRepository();

            if (File.Exists(path) == false)
                return Array.Empty<ConfigModelDeck>();

            LogExt.LogReadFile(path);
            var fileContent = File.ReadAllText(path);
            var jsonResult = JsonConvert.DeserializeObject<ConfigRootDecks>(fileContent)!.decks;
            var dictValues = jsonResult.GroupBy(i => i.Id).ToDictionary(i => i.Key, i => i.Last()).Select(i => i.Value).ToArray();

            foreach (var c in dictValues.SelectMany(d => d.Cards))
            {
                // To not use the weird UNCOMMON Doom Blade
                if (c.GrpId == 54017)
                    c.GrpId = 77507;
            }

            ////////////////////////////////////// TEMP ///////////////////
            // var allCards = cacheCards.Get();
            foreach (var d in jsonResult.Where(i => i.Cards == null))
            {
                var cardsMain = d.CardsMain.Select(i => new DeckCard(dictCards[i.Key], i.Value, DeckCardZoneEnum.Deck));
                var cardsSideboard = d.CardsSideboard.Select(i => new DeckCard(dictCards[i.Key], i.Value, DeckCardZoneEnum.Sideboard));
                var cards = cardsMain.Union(cardsSideboard).ToArray();
                // Migrate to new model
                d.Cards = cards.Select(i => new DeckCardRaw
                {
                    GrpId = i.Card.GrpId,
                    Amount = i.Amount,
                    Zone = i.Zone,
                }).ToArray();
            }
            //File.WriteAllText(path + "2", JsonConvert.SerializeObject(new ConfigRootDecks { decks = jsonResult }));
            ////////////////////////////////////////////////////////////////

            Log.Information("{nbDecks} decks to load", dictValues.Length);

            var validDecks = dictValues
                .Where(i => i.Cards.All(x => dictCards.ContainsKey(x.GrpId)))
                .ToArray()
                ;

            Log.Information("{nbDecks} valid decks", validDecks.Length);

            foreach (var c in dictValues.SelectMany(d => d.Cards))
            {
                // 2021-12-09: this seems obsolete
                // Lazy fix: Nightmare
                if (dictCards.ContainsKey(c.GrpId) && dictCards[c.GrpId].Name == "Nightmare")
                    c.GrpId = 75495;
            }

            Parallel.ForEach(validDecks, (d) =>
            {
                try
                {
                    //var cardsCommander = d.CardCommander == default ? new DeckCard[0] : new[] { new DeckCard(new CardWithAmount(dictCards[d.CardCommander], 1), DeckCardZoneEnum.Commander) };

                    //var cardsMain = d.CardsMain
                    //    .GroupBy(i => i.Key)
                    //    .ToDictionary(i => i.Key, i => i.Sum(x => x.Value))
                    //    .Select(i => new DeckCard(new CardWithAmount(dictCards[i.Key], i.Value), DeckCardZoneEnum.Deck));

                    //var cardsSideboard = d.CardsSideboard
                    //    .GroupBy(i => i.Key)
                    //    .ToDictionary(i => i.Key, i => i.Sum(x => x.Value))
                    //    .Select(i => new DeckCard(new CardWithAmount(dictCards[i.Key], i.Value), DeckCardZoneEnum.Sideboard));

                    //var deckCards = cardsCommander.Union(cardsMain).Union(cardsSideboard).ToArray();
                    var deckCards = d.Cards.Select(i => new DeckCard(dictCards[i.GrpId], i.Amount, i.Zone)).ToArray();

                    d.Deck = new Deck(d.Name, new ScraperType(d.ScraperTypeId), deckCards);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debugger.Break();
                    Log.Fatal(ex, "ERROR WHILE LOADING DECKS");
                }
            });

            Log.Information("{nbDecks} decks loaded", dictValues.Count(i => i.Deck != null));

            return validDecks;
        }

        public IReadOnlyCollection<ConfigModelDeck> Get()
        {
            return cache.Get();
        }

        public void ReloadDecks()
        {
            cache.Reload();
        }

        /// <summary>
        /// this method is not thread-safe and should be called from max. one thread!
        /// </summary>
        public void AddDecks(ICollection<ConfigModelDeck> newDecks)
        {
            Log.Information($"AddDecks: {newDecks.Count} decks");

            if (newDecks.Count > 0)
            {
                var decksDict = Get()
                    //// Remove weird decks where the sideboard is in the main deck
                    //.Where(i => (i.CardsMain.Sum(x => x.Value) == 75 && i.CardsSideboard.Sum(x => x.Value) == 0) == false)
                    .ToDictionary(i => i.Id, i => i);

                foreach (var d in newDecks)
                    decksDict[d.Id] = d;

                cache.Set(decksDict.Values);

                File.WriteAllText(path, JsonConvert.SerializeObject(new ConfigRootDecks { decks = Get() }));
            }
        }

        public void Reset()
        {
            cache.Set(Array.Empty<ConfigModelDeck>());
        }
    }
}