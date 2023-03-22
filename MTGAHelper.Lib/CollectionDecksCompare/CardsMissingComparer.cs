using MTGAHelper.Entity;
using MTGAHelper.Entity.CollectionDecksCompare;
using MTGAHelper.Entity.Config.Users;
using MTGAHelper.Entity.Services;
using MTGAHelper.Lib.Config.Users;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MTGAHelper.Lib.CollectionDecksCompare
{
    public interface ICardsMissingComparer
    {
        ICardsMissingComparer Init(IImmutableUser immutableUser, IEnumerable<CardWithAmount> userCollection);

        CardsMissingResult Compare(ICollection<IDeck> decks, IEnumerable<IDeck> decksUserDefined, IEnumerable<IDeck> mtgaDecks);
    }

    public class CardsMissingComparer : ICardsMissingComparer
    {
        private IImmutableUser user;
        private IReadOnlyDictionary<string, CardWithAmount> collection;
        private List<CardRequiredInfo> computedData;

        string userId => user.Id;

        private Dictionary<string, bool> decksTracked = new Dictionary<string, bool>();

        private System.Diagnostics.Stopwatch watch;
        private Dictionary<string, float> loadingTimes = new Dictionary<string, float>();
        private readonly BasicLandIdentifier basicLandIdentifier;

        public CardsMissingComparer(
            BasicLandIdentifier basicLandIdentifier
            )
        {
            this.basicLandIdentifier = basicLandIdentifier;
        }

        public ICardsMissingComparer Init(IImmutableUser immutableUser, IEnumerable<CardWithAmount> userCollection)
        {
            this.user = immutableUser;
            collection = userCollection
                .GroupBy(i => i.Card.Name)
                .Select(i => new CardWithAmount(i.Last().Card, i.Sum(x => x.Amount)))
                .ToDictionary(i => i.Card.Name, i => i);

            //var c = collection.Where(i => i.Card.name == "Opt").ToArray();
            //var c2 = collection.Where(i => i.Card.name == "Lightning Strike").ToArray();
            //if (c.Length > 1 || c2.Length > 1)
            //{
            //    System.Diagnostics.Debugger.Break();
            //}

            return this;
        }

        public CardsMissingResult Compare(ICollection<IDeck> decks, IEnumerable<IDeck> decksUserDefined, IEnumerable<IDeck> mtgaDecks)
        {
            if (collection == null || decks == null)
            {
                return new CardsMissingResult(Array.Empty<CardRequiredInfo>(), decksTracked);
            }

            Log.Debug("User {userId} comparing with scrapers: {scrapersActive}", userId, string.Join(", ", user.ScrapersActive));

            IDeck[] decksToCompare = null;
            //var t = decks.Where(i => i == null || i.ScraperType == null || user.ScrapersActive == null).ToArray();
            try
            {
                var deckList = decks
                    .Union(decksUserDefined)
                    .Union(mtgaDecks)
                    .Where(i => i != null);   // Added for comparisons when server just started and is still loading decks;

                decksToCompare = deckList
                    .Where(i => user.isDebug || GetPriorityFactor(i) != 0f)
                    .Where(i => user.isDebug || i.ScraperType.Type == ScraperTypeEnum.UserCustomDeck || user.ScrapersActive.Contains(i.ScraperType.Id))
                    //.Where(i => user.PriorityByDeckId.ContainsKey(i.Id) == false || user.PriorityByDeckId[i.Id] > 0)
                    //.Where(i => i.Cards.Any())
                    .ToArray();

                //var test = deckList.GroupBy(i => i.Id)
                //    .Where(i => i.Count() > 1)
                //    .ToArray();

                ////////try
                ////////{
                //////decksTracked = deckList
                //////    .GroupBy(i => i.Id)
                //////    .ToDictionary(i => i.Key,
                //////        // Tracked: Either Scraper active or User custom
                //////        //          AND priority != 0
                //////        i => i user.IsTracked(i));
                ////////}
                ////////catch (Exception ex)
                ////////{
                ////////    System.Diagnostics.Debugger.Break();
                ////////    var test = decks.Where(i => i.Id == "aetherhub-user_mtgarenaoriginaldecks-arenastandard_2891280397").ToArray();
                ////////}
                //// PATCHEROO
                decksTracked = deckList
                    .GroupBy(i => i.Id)
                    .ToDictionary(i => i.Key,
                        // Tracked: Either Scraper active or User custom
                        //          AND priority != 0
                        i => user.isDebug || user.IsTracked(i.First()));
            }
            catch (Exception ex)
            {
                Log.Error("ErRoR");
                //var tt = decks.Where(i => i.Id == "aetherhub-meta-standard5ec279dbb828fef15d6e222e469dd9ade915041ffe19a556423a2262251d3902").ToArray();
                throw;
            }

            Log.Debug("User {userId} comparing {nbDecks} decks", userId, decksToCompare.Length);

            watch = System.Diagnostics.Stopwatch.StartNew();
            computedData = ComputeIndividualCardsRequiredData(decksToCompare).ToList();
            watch.Stop();
            loadingTimes["Compute"] = watch.ElapsedMilliseconds / 1000.0f;

            watch = System.Diagnostics.Stopwatch.StartNew();
            var res = new CardsMissingResult(computedData, decksTracked);
            watch.Stop();
            loadingTimes["CreateResult"] = watch.ElapsedMilliseconds / 1000.0f;

            //watch = System.Diagnostics.Stopwatch.StartNew();
            //res.ApplyCompareResult(decksToCompare);
            //watch.Stop();
            //loadingTimes["ApplyCompareResult"] = watch.ElapsedMilliseconds / 1000.0f;

            return res;
        }

        private IEnumerable<CardRequiredInfo> ComputeIndividualCardsRequiredData(IEnumerable<IDeck> decks)
        {
            //if (decks.Any(i => i.Id == "cbe5cfdf7c2118a9c3d78ef1d684f3afa089201352886449a06a6511cfef74a7"))
            //{
            //    System.Diagnostics.Debugger.Break();
            //}
            //var daa = decks.Count(i => i.Name == "Azorius High Alert");

            watch = System.Diagnostics.Stopwatch.StartNew();

            IEnumerable<CardRequiredInfo> ProcessDeck(IDeck d)
            {
                //if (d.Name == "Test Sideboard")
                //{
                //    System.Diagnostics.Debugger.Break();
                //}

                if (d is Deck)
                    return ProcessDeckNormal((Deck)d);
                //else if (d is DeckAverageArchetype)
                //    return ProcessDeckAverageArchetype((DeckAverageArchetype)d);
                else
                    return Enumerable.Empty<CardRequiredInfo>();
            }

            var cardsRequired = decks
#if !DEBUG
                .AsParallel()
#endif
                .SelectMany(ProcessDeck).ToList();

            watch.Stop();
            loadingTimes["ComputeIndividualCardsRequiredData"] = watch.ElapsedMilliseconds / 1000.0f;

            return cardsRequired;
        }

        private (int nbOwned, int nbMissing) GetInfo(CardWithAmount c)
        {
            var nbOwned = 0;
            var nbMissing = Math.Min(4, c.Amount);

            //if (c.Card.name.EndsWith(" (a)") || c.Card.name.EndsWith(" (b)"))
            //{
            //    // Guildgates case
            //    var nameNoAlteration = c.Card.name.Substring(0, c.Card.name.Length - 4);
            //    var altA = $"{nameNoAlteration} (a)";
            //    var altB = $"{nameNoAlteration} (b)";
            //    var amount = (collection.ContainsKey(altA) ? collection[altA].Amount : 0) +
            //                 (collection.ContainsKey(altB) ? collection[altB].Amount : 0);

            //    nbOwned = amount;
            //    nbMissing = Math.Max(0, c.Amount - nbOwned);
            //}
            //else
            {
                //if (c.Card.name == "Llanowar Elves")
                //    System.Diagnostics.Debugger.Break();

                // Normal case
                if (collection.ContainsKey(c.Card.Name))
                {
                    nbOwned = collection[c.Card.Name].Amount;
                    nbMissing = Math.Max(0, Math.Min(4, c.Amount) - nbOwned);
                }
            }

            return (nbOwned, nbMissing);
        }

        //private ICollection<CardRequiredInfo> ProcessDeckAverageArchetype(DeckAverageArchetype d)
        //{
        //    List<CardRequiredInfo> ret = new List<CardRequiredInfo>();

        //    ret.AddRange(ProcessDeckNormal(new Deck(d.Name, d.ScraperType, d.Cards.All)));

        //    foreach (var c in d.CardsMainOther
        //        .Where(i => i.type.StartsWith("Basic Land") == false))
        //    {
        //        var info = ProcessCardMainOther(c, d);
        //        ret.Add(info);
        //    }

        //    return ret;
        //}

        private ICollection<CardRequiredInfo> ProcessDeckNormal(Deck deck)
        {
            //if (deck.Name == "Artifact Bo1") System.Diagnostics.Debugger.Break();

            //if (deck.Id == "cbe5cfdf7c2118a9c3d78ef1d684f3afa089201352886449a06a6511cfef74a7")
            //if (deck.Name == "Azorius High Alert")
            //    System.Diagnostics.Debugger.Break();

            //if (deck.Cards.All.Any(i => i.Card.name.StartsWith("Graveyard Marsh"))) System.Diagnostics.Debugger.Break();

            return deck.Cards.All
                .Where(i => basicLandIdentifier.IsBasicLand(i.Card) == false)
                .Select(i => ProcessCardNormal(i, deck))
                .ToArray();
        }

        private CardRequiredInfo ProcessCardNormal(DeckCard c, Deck deck)
        {
            //if (c.Card.name.Contains("Dub")) System.Diagnostics.Debugger.Break();

            var (nbOwned, nbMissing) = GetInfo(c);

            if (c.Zone == DeckCardZoneEnum.Sideboard)
            {
                if (deck.Cards.QuickCardsMain.ContainsKey(c.Card.GrpId))
                {
                    // Account for the same card in the main
                    var sameCardInMain = deck.Cards.QuickCardsMain[c.Card.GrpId];

                    //if (sameCardInMain.Amount + c.Amount > 4)
                    //    System.Diagnostics.Debugger.Break();

                    nbMissing = Math.Max(0, Math.Min(sameCardInMain.Amount, sameCardInMain.Amount + c.Amount - nbOwned));
                }
            }

            var res = new CardRequiredInfo(c.Card, false, c.Zone == DeckCardZoneEnum.Sideboard, deck, c.Amount, nbOwned, nbMissing,
                GetPriorityFactor(deck), userId, user.Weights.OrDefaultValues());

            //if (configApp.CardsNoWeight.Contains(c.Card.name))
            //    res.SetMissingWeight(0);

            return res;
        }

        //private CardRequiredInfo ProcessCardMainOther(DeckAverageArchetypeOtherMainCard c, DeckAverageArchetype deck)
        //{
        //    var nbOwned = GetInfo(new CardWithAmount(c, 0)).nbOwned;
        //    return new CardRequiredInfo(c, true, false, deck, 0, nbOwned, 0, GetPriorityFactor(deck),
        //        userId, user.Weights.OrDefaultValues());
        //}

        private float GetPriorityFactor(IDeck deck)
        {
            var deckId = deck.Id;
            if (user.PriorityByDeckId?.ContainsKey(deckId) == true)
                return user.PriorityByDeckId[deckId];

            return 1.0f;
        }
    }
}