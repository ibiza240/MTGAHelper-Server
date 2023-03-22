using AutoMapper;
using Minmaxdev.Cache.Common.Service;
using MtgaDecksPro.Cards.BootstrapCardsBuilding.AssemblyConfig;
using MtgaDecksPro.Cards.BootstrapCardsBuilding.Entity;
using MtgaDecksPro.Cards.BootstrapCardsBuilding.Entity.Mtga;
using MtgaDecksPro.Cards.BootstrapCardsBuilding.Entity.Scryfall;
using MtgaDecksPro.Cards.Entity;
using MtgaDecksPro.Cards.Entity.Service;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MtgaDecksPro.Cards.BootstrapCardsBuilding.Service
{
    public class CardsBuilder
    {
        private readonly IMapper mapper;
        private readonly ILogger logger;
        private readonly ICacheHandler<SetsInfo> cacheSets;
        private readonly ICacheHandler<List<HistoricAnthologyCards>> cacheHistoricAnthologies;
        private readonly CacheManualDataModel cacheManualData;
        private readonly ReaderMtgaDataCards readerMtgaDataCards;
        private readonly ScryfallSeeker scryfallSeeker;
        private readonly BasicLandDetectorFromTypeLine basicLandDetector;
        private readonly ICacheHandler<List<ScryfallModelRootObjectExtended>> dataHandlerScryfallCards;

        private Dictionary<string, string> ExceptionsCardsNonCraftable = new Dictionary<string, string>
        {
            ["gr5"] = "Ral, Izzet Viceroy",
            ["gr8"] = "Vraska, Golgari Queen",
        };

        private ICollection<string> CardsToIgnore = new[]
        {
            "Factory of Momir Vig",
            "Maelstrom Nexus Emblem",
            "Omniscience Emblem",
            "Treasure Token Factory",
            "Pandemonium Emblem",
            "Giant Monsters Emblem",
            "Overflowing Counters",
            "Landfall Satchel",
            "Oko's Whims",
            "Extra Life",
            "Mastermind Emblem",
            "Turbo Magic Emblem",
            "Happy Yargle Day!"
        };

        private Dictionary<string, string> typos = new Dictionary<string, string>
        {
            { "Lurrus of the Dream Den", "Lurrus of the Dream-Den" },
            { "Sol'kanar the Tainted", "Sol'Kanar the Tainted" },
        };

        public CardsBuilder(
            ILogger logger,
            ICacheHandler<SetsInfo> cacheSets,
            MemoryCacheManualData cacheManualData,
            ICacheHandler<List<HistoricAnthologyCards>> cacheHistoricAnthologies,
            ReaderMtgaDataCards readerMtgaDataCards,
            ScryfallSeeker scryfallSeeker,
            BasicLandDetectorFromTypeLine basicLandDetector,
            ICacheHandler<List<ScryfallModelRootObjectExtended>> dataHandlerScryfallCards
            )
        {
            this.mapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MapperProfileCardsBuilding>();
            }).CreateMapper();

            this.logger = logger;
            this.cacheSets = cacheSets;
            this.cacheHistoricAnthologies = cacheHistoricAnthologies;
            this.cacheManualData = cacheManualData.Get().Result;
            this.scryfallSeeker = scryfallSeeker;
            this.basicLandDetector = basicLandDetector;
            this.dataHandlerScryfallCards = dataHandlerScryfallCards;
            this.readerMtgaDataCards = readerMtgaDataCards;
        }

        public async Task<List<CardWriteable>> GetFullCards()
        {
            var historicAnthologies = await cacheHistoricAnthologies.Get();

            var result = new List<CardWriteable>();
            foreach (var card in await GetCardsToProcess())
            {
                //if (card.IdArena == 79429)
                //    Debugger.Break();

                // Manual typos replacement
                card.Name = typos.ContainsKey(card.Name) ? typos[card.Name] : card.Name;

                try
                {
                    await AddCardToResult(card, result);
                }
                catch (Exception ex)
                {
                    Debugger.Break();
                }
            }

            // Set the RelatedTokenCards
            var resultsCopy = result.ToArray();
            foreach (var c in result)
            {
                if (c.IsToken)
                {
                    c.RelatedTokenCards = resultsCopy
                        .Where(i => c.SetArena == i.SetArena)
                        .Where(i =>
                            new Regex($"{c.Name}.*?token").IsMatch(i.OracleText ?? "") ||
                            new Regex($"named {c.Name}").IsMatch(i.OracleText ?? "") ||
                            new Regex($"{c.Power}/{c.Toughness}.*?{c.Name}").IsMatch(i.OracleText ?? "") ||
                            new Regex($"{c.Name}:").IsMatch(i.OracleText ?? "")
                        )
                        .Select(i => i.IdArena)
                        .ToArray();
                }
                else
                {
                    c.RelatedTokenCards = resultsCopy
                        .Where(i => i.IsToken)
                        .Where(i => c.SetArena == i.SetArena)
                        .Where(i =>
                            new Regex($"{i.Name}.*?token").IsMatch(c.OracleText ?? "") ||
                            new Regex($"named {i.Name}").IsMatch(c.OracleText ?? "") ||
                            new Regex($"{i.Power}/{i.Toughness}.*?{i.Name}").IsMatch(c.OracleText ?? "") ||
                            new Regex($"{i.Name}:").IsMatch(c.OracleText ?? "")
                        )
                        .Select(i => i.IdArena)
                        .ToArray();
                }
            }

            return result;
        }

        private async Task<IEnumerable<CardWriteable>> GetCardsToProcess()
        {
            var mtgaCards = await readerMtgaDataCards
                .GetMtgaCards();

            var historicAnthologies = await cacheHistoricAnthologies.Get();

            var scryfallCardsByName = dataHandlerScryfallCards.Get().Result
                .OrderBy(i => i.collector_number)
                .GroupBy(i => i.name)
                .ToDictionary(i => i.Key, i => i.First());

            // Cards from Arena
            var cards = mtgaCards
                .Where(i =>
                {
                    try
                    {
                        return CardsToIgnore.Contains(i.Name) == false;
                    }
                    catch (Exception ex)
                    {
                        Debugger.Break();
                        return default;
                    }
                })
                .Select(i =>
                {
                    try
                    {
                        return mapper.Map<CardWriteable>(i);
                    }
                    catch (Exception ex)
                    {
                        Debugger.Break();
                        return default;
                    }
                })
                // Ignore these cards for now
                .Where(i => i.IdArena != 54017) // Unobtainable UNCOMMON Doom Blade
                .ToList();

            var cardsSoFar = cards
                .GroupBy(i => i.Name).ToDictionary(i => i.Key, i => i.First());

            //var test = historicAnthologies.SelectMany(i => i.Cards)
            //    .Select(i => scryfallCardsByName[i])
            //    .Select(card => new
            //    {
            //        card,
            //        type_line = card.type_line == null,
            //        power = card.power == null,
            //        toughness = card.toughness == null,
            //        flavor_text = card.flavor_text == null,
            //        set = card.set == null,
            //    })
            //    .Where(i => i.type_line || i.set || i.power || i.toughness || i.flavor_text)
            //    .ToArray();

            //if (test.Any())
            //    Debugger.Break();

            // Cards from Anthologies
            cards.AddRange(historicAnthologies
                .SelectMany(i => i.Cards.Select(x => mapper.Map<CardWriteable>(scryfallCardsByName[x])))
                .Where(i => cardsSoFar.ContainsKey(i.Name) == false)
            );

            // Other cards?
            // USED WHEN FUTURE CARDS ARE AVAILABLE (e.g. next set spoiled)
            //cards.AddRange(scryfallCardsByName.Values.Where(i => i.set.EndsWith("stx"))
            //    .Select(i => mapper.Map<Card>(i))
            //);

            return cards;
        }

        private async Task AddCardToResult(CardWriteable c, List<CardWriteable> result)
        {
            //if (c.Name.Contains("Ghost Lantern"))
            //    Debugger.Break();

            //if (c.Name == "Construct" && c.SetArena == "KLR")
            //if (c.Name.Contains("homunculus", StringComparison.InvariantCultureIgnoreCase))
            //    Debugger.Break();

            var found = scryfallSeeker.TryFillScryfallInfo(c);
            if (found == false)
            {
                found = cacheManualData.Cards.ContainsKey(c.Name);
                if (found)
                {
                    var m = cacheManualData.Cards[c.Name];
                    c.TypeLine = m.TypeLine ?? Constants.Unknown;
                    c.ManaCost = m.ManaCost;
                    c.OracleText = m.OracleText;
                    c.SetScryfall = m.SetScryfall ?? "ana";
                    c.ImageCardUrl = m.ImageCardUrl;
                    c.ImageArtUrl = m.ImageArtUrl;
                    //c.IsCollectible = false;
                    //c.Layout = m.layout;
                    logger.Warning("Card {name} ({setArena} {number}) from {artist} found in Manual data", c.Name, c.SetArena, c.Number, c.ArtistCredit);
                }
                else
                    logger.Error("Card {name} ({setArena} {number}) from {artist} not found in Scryfall", c.Name, c.SetArena, c.Number, c.ArtistCredit);
            }

            //if (c.Name == "Increasing Vengeance")
            //    Debugger.Break();

            //if (c.Name == "Vraska, Golgari Queen")
            //    Debugger.Break();

            //if (c.IdArena == 70141)
            //    Debugger.Break();

            if (found)
            {
                ////if (c.Name == "Myr Enforcer")
                //if (c.Name == "Tibalt, Wicked Tormenter" || c.Name == "Angel of Eternal Dawn")
                //    Debugger.Break();

                c.IsStyle = await IsStyle(c, result);

                c.IsCraftable = await IsCraftable(c);

                // Can only be called after the Set property has been set
                c.IsInBooster = await IsInBooster(c);

                c.IsCollectible = await IsCollectible(c);

                if (c.SetArena == "JMP" && c.IsCollectible == false && c.IsToken == false) Debugger.Break();

                result.Add(c);
                logger.Debug("Card {name} ({setArena} {number}) added", c.Name, c.SetArena, c.Number, c.ArtistCredit);
            }
        }

        private async Task<bool> IsStyle(CardWriteable c, List<CardWriteable> results)
        {
            var setsInfo = await cacheSets.Get();
            var historicAnthologies = await cacheHistoricAnthologies.Get();

            //if (c.SetScryfall == "ana" && c.Name == "Plains")
            //    Debugger.Break();

            // Not a number is assumed always a style
            if (int.TryParse(c.Number, out int iNumber) == false)
                return true;

            // Historic Anthologies cards are not style
            if (historicAnthologies.Any(i => i.Cards.Any(x => x == c.Name)))
                return false;

            // Weird sets
            if (setsInfo.SetsHistoricAll.Any(i => i == c.SetScryfall) == false)
            {
                // is assumed always a style
                // but cards that have no other equivalent in the set are not styles
                return results.Where(i => i.SetArena == c.SetArena).Count(i => i.Name == c.Name) > 0;
            }

            var setInfo = setsInfo.SetsByCodeScryfall[c.SetScryfall];

            // Promo cards special treatment
            if (setInfo.PromoCardNumber == c.Number)
                return setInfo.PromoCardIsStyle;

            if (iNumber > setInfo.NbCards)
            {
                // Is a stye if Number is greater than the normal set size
                // but cards that have no other equivalent in the set are not styles
                return results.Where(i => i.SetArena == c.SetArena).Count(i => i.Name == c.Name) > 0;
            }
            else
                return false;
        }

        private async Task<bool> IsCraftable(CardWriteable c)
        {
            var setsInfo = await cacheSets.Get();
            var historicAnthologies = await cacheHistoricAnthologies.Get();

            //if (c.Name == "Myr Enforcer")
            //    Debugger.Break();

            // Weird craftable cards
            var craftable = new[]
            {
                "Rhys the Redeemed",
                "The Gitrog Monster",
                "Thalia, Guardian of Thraben",
                "Talrand, Sky Summoner",
                "Hanna, Ship's Navigator",
            };

            // Tokens, Basic lands, Manually set exceptions
            // are not craftable
            if (c.IsToken ||
                basicLandDetector.IsBasicLandFromTypeLine(c.TypeLine) ||
                ExceptionsCardsNonCraftable.Any(i => i.Key == c.Number && i.Value == c.Name))
                return false;

            // Cannot craft a card that is not found within all "normal" sets
            // nor found within historic anthologies
            if (setsInfo.SetsHistoricAll.Any(i => i == c.SetScryfall.Substring(c.SetScryfall.Length - 3, 3)) == false &&
                historicAnthologies.Any(i => i.Cards.Any(x => x == c.Name)) == false
            )
            {
                return false;
            }

            return c.IsPrimaryCard;
        }

        private async Task<bool> IsInBooster(CardWriteable c)
        {
            //if (c.Name.StartsWith("Benalish Knight-Counselor"))
            //    Debugger.Break();

            var setsInfo = await cacheSets.Get();

            //if (c.Name == "Thief of Sanity")
            //    Debugger.Break();

            //if (c.SetScryfall == "khm")
            //    Debugger.Break();

            //c.IsCraftable = !c.IsToken && c.IsCollectible && c.TypeLine.StartsWith("Basic Land") == false;

            var isM20CommonDual = c.SetScryfall == "M20" && (
                c.Name == "Bloodfell Caves" ||
                c.Name == "Blossoming Sands" ||
                c.Name == "Dismal Backwater" ||
                c.Name == "Jungle Hollow" ||
                c.Name == "Rugged Highlands" ||
                c.Name == "Scoured Barrens" ||
                c.Name == "Swiftwater Cliffs" ||
                c.Name == "Thornwood Falls" ||
                c.Name == "Tranquil Cove" ||
                c.Name == "Wind-Scarred Crag"
            );
            if (isM20CommonDual)
                return false;

            // Special art, not actual card, but weirdly same card number as the legit one
            if (c.IdArena == 70141)
                return false;

            return
                c.SetArena != "JMP" &&
                c.SetArena != "J21" &&
                c.SetArena != "ANA" &&
                c.SetArena != "ANB" &&
                c.IsCraftable &&
                setsInfo.SetsByCodeScryfall.ContainsKey(c.SetScryfall) &&
                int.TryParse(c.Number, out int number) &&
                number <= setsInfo.SetsByCodeScryfall[c.SetScryfall].NbCards
                ;
        }

        private async Task<bool> IsCollectible(CardWriteable c)
        {
            var setsInfo = await cacheSets.Get();
            var historicAnthologies = await cacheHistoricAnthologies.Get();

            //if (c.Name == "Teyo, Aegis Adept")
            //    Debugger.Break();

            // Styles are always collectible
            if (c.IsStyle)
                return true;

            // Collectible if in a normal set or from Anthologies
            return c.IsPrimaryCard &&
                (setsInfo.SetsHistoricAll.Any(i => i == c.SetScryfall.Substring(c.SetScryfall.Length - 3, 3)) || historicAnthologies.Any(i => i.Cards.Any(x => x == c.Name)));
        }
    }
}