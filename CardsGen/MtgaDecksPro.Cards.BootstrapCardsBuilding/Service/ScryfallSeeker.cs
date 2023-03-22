using Minmaxdev.Cache.Common.Service;
using MtgaDecksPro.Cards.BootstrapCardsBuilding.Entity;
using MtgaDecksPro.Cards.BootstrapCardsBuilding.Entity.Scryfall;
using MtgaDecksPro.Cards.Entity;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MtgaDecksPro.Cards.BootstrapCardsBuilding.Service
{
    public class ScryfallSeeker
    {
        private Dictionary<string, string> dictSetArenaToScryfall;
        private Dictionary<int, string> cardFaces = new Dictionary<int, string>();
        private readonly ILogger logger;
        private readonly ICacheHandler<List<ScryfallModelRootObjectExtended>> dataHandlerScryfallCards;
        private readonly ICacheHandler<SetsInfo> cacheSets;

        private List<ScryfallModelRootObjectExtended> scryfallCards;

        public ScryfallSeeker(
            ILogger logger,
            ICacheHandler<List<ScryfallModelRootObjectExtended>> dataHandlerScryfallCards,
            ICacheHandler<SetsInfo> cacheSets
            )
        {
            this.logger = logger;
            this.dataHandlerScryfallCards = dataHandlerScryfallCards;
            this.cacheSets = cacheSets;
        }

        private bool isSameSet(string setCard, string setScryfall)
        {
            if (setCard == "y22")
            {
                return setScryfall.StartsWith("y") && setScryfall.Length == 4;
            }

            return setCard == setScryfall;
        }

        public bool TryFillScryfallInfo(CardWriteable c)
        {
            //if (c.Name.StartsWith("Rahilda"))
            //    Debugger.Break();

            ////if (c.Name == "Myr Enforcer")
            //if (c.Name == "Tibalt, Wicked Tormenter" || c.Name == "Angel of Eternal Dawn")
            //    Debugger.Break();

            scryfallCards = dataHandlerScryfallCards.Get().Result;
            var sets = cacheSets.Get().Result;
            var test = scryfallCards.Where(i => i.name.Contains("Ghost Lantern"));

            if (c.Name == "Ghost Lantern" && c.ArtistCredit == "Kari Christensen")
                c.ArtistCredit = "Julian Kok Joon Wen";

            dictSetArenaToScryfall = sets.SetsByCodeScryfall.Values
                .Where(i => i.CodeArena != null)
                .ToDictionary(i => i.CodeArena, i => i.CodeScryfall);

            var matchMethod = CardMatchMethodEnum.Unknown;

            //if (c.Name == "Collected Company" || c.Name == "Archon of Sun's Grace")
            //    Debugger.Break();

            // Dirty fix to find the correct image for this euro land
            if (c.IdArena == 75896)
                c.Number = "2";

            c.Name = c.Name.Replace("///", "//");
            var set = DetermineSet(c);
            ScryfallModelRootObjectExtended scryfallCard = null;

            // Ready to start!

            // Tokens
            if (c.IsToken)
            {
                if (c.Name == "Zombie Army")
                {
                    // To get the art variants correctly
                    matchMethod = CardMatchMethodEnum.GrpId;
                    switch (c.IdArena)
                    {
                        case 69735: // Set WAR Art 1
                        case 76270: // Set MH2
                            scryfallCard = scryfallCards.Single(i => i.name == c.Name && i.collector_number == "8");
                            break;

                        case 69736: // Set WAR Art 2
                            scryfallCard = scryfallCards.Single(i => i.name == c.Name && i.collector_number == "9");
                            break;

                        case 69737: // Set WAR Art 3
                            scryfallCard = scryfallCards.Single(i => i.name == c.Name && i.collector_number == "10");
                            break;

                        case 70469: // Set ArenaSUP
                            scryfallCard = scryfallCards.Single(i => i.name == c.Name && i.collector_number == "8");
                            break;
                    }
                }
                else
                {
                    var setToken = set == "klr" ? "kld" : set == "akr" ? "akh" : set;

                    matchMethod = CardMatchMethodEnum.NameAndSetAndNumberAndArtist;
                    scryfallCard = scryfallCards.FirstOrDefault(i => i.name == c.Name && (i.set == setToken || i.set == $"t{setToken}") && i.collector_number == c.Number && IsSameArtist(c.ArtistCredit, i.artist));

                    if (scryfallCard == null)
                    {
                        matchMethod = CardMatchMethodEnum.NameAndSetAndArtist;
                        scryfallCard = scryfallCards.FirstOrDefault(i => i.name == c.Name && (i.set == setToken || i.set == $"t{setToken}") && IsSameArtist(c.ArtistCredit, i.artist));

                        if (scryfallCard == null)
                        {
                            matchMethod = CardMatchMethodEnum.NameAndArtist;
                            scryfallCard = scryfallCards.FirstOrDefault(i => i.name == c.Name && IsSameArtist(c.ArtistCredit, i.artist));

                            //if (scryfallCard == null)
                            //{
                            //    matchMethod = CardMatchMethodEnum.NameAndSetAndNumber;
                            //    var test = scryfallCards.Where(i => i.id.ToString() == "7d13a93a-a43d-4cf5-8300-8341f3b7f1b1");
                            //    scryfallCard = scryfallCards.FirstOrDefault(i => i.name == c.Name && (i.set == setToken || i.set == $"t{setToken}") && i.collector_number == c.Number);
                            //}
                        }
                    }
                }

                if (scryfallCard != null)
                {
                    //if (scryfallCard.set.Length > 3)
                    //    scryfallCard.set = scryfallCard.set.Substring(1, scryfallCard.set.Length - 1);
                    ////else
                    ////    Debugger.Break();

                    return FillScryfallInfo(c, scryfallCard, matchMethod);
                }
                else if (
                    c.SetArena != "ANA" && c.SetArena != "J21" && c.SetArena != "ArenaSUP" &&
                    c.Name != "Phyrexian Myr" && c.Name != "Phyrexian Golem" && c.Name != "Day" && c.Name != "Night" &&
                    c.IsToken == false &&
                    c.Name != "Abundant Falls")
                    Debugger.Break();
            }

            // Cards with different faces (split, flip, adventures, ...)
            matchMethod = CardMatchMethodEnum.NameAndSetAndArtist;
            //var test = ScryfallCards.Where(i => i.card_faces?.Any(x => x.name == c.Name) == true && i.set == set).ToArray();
            scryfallCard = scryfallCards.FirstOrDefault(i => i.card_faces?.Any(x => x.name == c.Name) == true && isSameSet(set, i.set) && IsSameArtist(c.ArtistCredit, i.artist));
            if (scryfallCard != null)
            {
                // Try to be more precise by including the number
                var scryfallCard2 = scryfallCards.FirstOrDefault(i => i.card_faces?.Any(x => x.name == c.Name) == true && isSameSet(set, i.set) && i.collector_number == c.Number && IsSameArtist(i.artist, c.ArtistCredit));
                if (scryfallCard2 != null)
                {
                    matchMethod = CardMatchMethodEnum.NameAndSetAndNumberAndArtist;
                    scryfallCard = scryfallCard2;
                }

                var face1 = scryfallCard.card_faces[0];
                var face2 = scryfallCard.card_faces[1];

                if (face1.name == c.Name)
                {
                    cardFaces.Add(c.IdArena, face2.name);
                    return FillScryfallInfoWithTwoCards(c, scryfallCard, face1, matchMethod);
                }
                else if (face2.name == c.Name)
                {
                    cardFaces.Add(c.IdArena, face1.name);
                    return FillScryfallInfoWithTwoCards(c, scryfallCard, face2, matchMethod);
                }
                else
                    Debugger.Break();
            }

            // Normal card, simplest case
            (scryfallCard, matchMethod) = GetNormalCard(c, set);
            if (scryfallCard != null)
                return FillScryfallInfo(c, scryfallCard, matchMethod);

            // Mythic edition (Like Vraska, Ral, ...)
            matchMethod = CardMatchMethodEnum.NameAndSetAndArtist;
            scryfallCard = scryfallCards.FirstOrDefault(i => i.name == c.Name && i.set == "med" && IsSameArtist(c.ArtistCredit, i.artist));
            if (scryfallCard != null)
                return FillScryfallInfo(c, scryfallCard, matchMethod);

            //// ANA only, pick an alternative
            //if (c.SetArena == "ANA")
            //{
            //    scryfallCard = scryfallCards.FirstOrDefault(i => i.name == c.Name && IsSameArtist(c.ArtistCredit, i.artist));
            //    if (scryfallCard != null)
            //        return FillScryfallInfo(c, scryfallCard);
            //}

            // Promo cards by Artist (for weird promo cards like ghalta, firemind's research, llanowar elves)
            matchMethod = CardMatchMethodEnum.NameAndSetAndArtist;
            scryfallCard = scryfallCards.FirstOrDefault(i => i.name == c.Name && i.set == $"p{set}" && IsSameArtist(c.ArtistCredit, i.artist));
            if (scryfallCard != null)
                return FillScryfallInfo(c, scryfallCard, matchMethod);

            // Normal card without artist (for godzilla series)
            matchMethod = CardMatchMethodEnum.NameAndSetAndNumber;
            scryfallCard = scryfallCards.FirstOrDefault(i => i.name == c.Name && isSameSet(set, i.set) && i.collector_number == c.Number);
            if (scryfallCard != null)
                return FillScryfallInfo(c, scryfallCard, matchMethod);

            // Last chance
            // Normal card, simplest case WITHOUT CARD NUMBER
            (scryfallCard, matchMethod) = GetNormalCard(c, set, false);
            if (scryfallCard != null)
                return FillScryfallInfo(c, scryfallCard, matchMethod);

            // Last last chance, by name and artist
            matchMethod = CardMatchMethodEnum.NameAndArtist;
            scryfallCard = scryfallCards.FirstOrDefault(i => i.name == c.Name && IsSameArtist(c.ArtistCredit, i.artist) /*&& i.lang == "en"*/);
            if (scryfallCard != null)
            {
                if (set == "iko" && c.ArtistCredit == "Kogado Studio" || set == "und" && c.Number == "96")
                    return FillScryfallInfo(c, scryfallCard, matchMethod);
                else if (c.Name == "Duress" && c.ArtistCredit == "Lawrence Snelly")
                {
                    // Special case, set is M19 incorrect, we match by name and artist after
                    Debugger.Break();
                }
                else if (
                    set != "akr" &&
                    set != "ana" &&
                    set != "anb" &&
                    set != "klr" &&
                    set != "j21" &&
                    set != "y22" &&
                    set != "hbg" &&
                    (c.Name == "Duress" && set == "m19") == false &&
                    c.Name != "Teferi's Ageless Insight" &&
                    c.Name != "Dungeon of the Mad Mage" &&
                    c.Name != "Lost Mine of Phandelver" &&
                    c.Name != "Tomb of Annihilation" &&
                    c.SetArena.StartsWith("y") == false
                    )
                    Debugger.Break();

                return FillScryfallInfo(c, scryfallCard, matchMethod);
            }

            //if (c.Name.Contains("Factory") == false && c.Name.Contains("Emblem") == false)
            //    Debugger.Break();

            return false;
        }

        /// <summary>
        ///  Because they are not always written exactly the same and don't use the same diacritics/characters
        /// </summary>
        private bool IsSameArtist(string mtga_artistCredit, string scryfall_artist)
        {
            if (mtga_artistCredit == null)
                return false;

            // Manual exceptions first
            if (
                mtga_artistCredit.Equals("Bob Ross", StringComparison.InvariantCultureIgnoreCase) && scryfall_artist.Equals("Jonas De Ro", StringComparison.InvariantCultureIgnoreCase) ||
                mtga_artistCredit.Equals("Bob Ross", StringComparison.InvariantCultureIgnoreCase) && scryfall_artist.Equals("Tony Roberts", StringComparison.InvariantCultureIgnoreCase) ||
                mtga_artistCredit.Equals("Bob Ross", StringComparison.InvariantCultureIgnoreCase) && scryfall_artist.Equals("Babyson Chen & Uzhen Lin", StringComparison.InvariantCultureIgnoreCase) ||
                mtga_artistCredit.Equals("Bob Ross", StringComparison.InvariantCultureIgnoreCase) && scryfall_artist.Equals("Zoltan Boros & Gabor Szikszai", StringComparison.InvariantCultureIgnoreCase)
            )
                return false;

            if (mtga_artistCredit == "Kogado Studio" && (
                scryfall_artist == "ヨロイコウジ" ||
                scryfall_artist == "Kotakan"
                )) return true;

            // Sometimes the MTGA artist is only the family name (and can be in full caps)
            if (scryfall_artist.Contains(mtga_artistCredit, StringComparison.InvariantCultureIgnoreCase))
                return true;

            if (mtga_artistCredit.Contains(scryfall_artist, StringComparison.InvariantCultureIgnoreCase) ||
                scryfall_artist.Contains(mtga_artistCredit, StringComparison.InvariantCultureIgnoreCase))
                return true;

            var sameLetters = (double)scryfall_artist.Count(i => mtga_artistCredit.Contains(i, StringComparison.InvariantCultureIgnoreCase));
            var sameLetters2 = (double)mtga_artistCredit.Count(i => scryfall_artist.Contains(i, StringComparison.InvariantCultureIgnoreCase));

            if ((double)sameLetters / mtga_artistCredit.Length > 0.7d && (double)sameLetters2 / mtga_artistCredit.Length > 0.7d)
                return true;
            else
                return false;
        }

        private bool FillScryfallInfo(CardWriteable c, ScryfallModelRootObjectExtended scryfallCard, CardMatchMethodEnum matchEnum)
        {
            //var text = scryfallCard.oracle_text;

            //if (scryfallCard.layout == "split")
            //    text = string.Join(" // ", scryfallCard.card_faces.Select(i => i.oracle_text));

            scryfallCard.UpdateCardWithInfo(c, scryfallCards);

            c.BuilderMethod = matchEnum;
            if (c.BuilderMethod < CardMatchMethodEnum.NameAndSetAndNumber)
            {
                if (c.BuilderMethod != CardMatchMethodEnum.NameAndArtist || scryfallCards.Count(i => i.name == c.Name && i.artist == c.ArtistCredit) > 1)
                    logger.Warning("Card {name} ({setArena} {number}) from {artist} with low match <{matchMethod}>", c.Name, c.SetArena, c.Number, c.ArtistCredit, c.BuilderMethod);
            }

            return true;
        }

        private bool FillScryfallInfoWithTwoCards(CardWriteable c, ScryfallModelRootObjectExtended scryfallCard, CardFace otherFace, CardMatchMethodEnum matchEnum)
        {
            scryfallCard.UpdateCardWithInfo(c, scryfallCards, otherFace.image_uris ?? scryfallCard.image_uris);

            c.TypeLine = otherFace.type_line;
            c.ManaCost = otherFace.mana_cost;
            c.OracleText = otherFace.oracle_text;

            c.BuilderMethod = matchEnum;
            return true;
        }

        private (ScryfallModelRootObjectExtended, CardMatchMethodEnum) GetNormalCard(CardWriteable c, string set, bool withCollectorNumber = true)
        {
            var matchMethod = withCollectorNumber ? CardMatchMethodEnum.NameAndSetAndNumberAndArtist : CardMatchMethodEnum.NameAndSetAndArtist;
            var scryfallCard = scryfallCards.FirstOrDefault(i =>
                i.name == c.Name &&
                i.set == set &&
                (withCollectorNumber == false || i.collector_number == c.Number) &&
                IsSameArtist(c.ArtistCredit, i.artist)
            );

            //if (scryfallCard == null
            //    && c.Name != "Collected Company" && c.Name != "Firemind's Research" && c.Name != "Llanowar Elves" && c.Name != "Ghalta, Primal Hunger" && c.Name != "Duress"
            //    && set != "ana" && set != "anb")
            //{
            //    // Try without artist match, weird case with Prophetic Prism in KLR
            //    // Scryfall artist as "Noah Bradley" but correct one is "Valera Lutfullina"
            //    scryfallCard = scryfallCards.FirstOrDefault(i =>
            //       i.name == c.Name &&
            //       i.set == set &&
            //       (withCollectorNumber == false || i.collector_number == c.Number)
            //   );

            //    if (scryfallCard != null && set != "klr" && set != "sld")
            //        Debugger.Break();
            //}

            if (scryfallCard != null)
                FillScryfallInfo(c, scryfallCard, matchMethod);
            else if (set == "jmp")
            {
                // Jumpstart Arena replacements
                scryfallCard = scryfallCards.FirstOrDefault(i => i.name == c.Name && i.set == $"a{set}" && i.collector_number == c.Number && IsSameArtist(c.ArtistCredit, i.artist));
                if (scryfallCard != null)
                    FillScryfallInfo(c, scryfallCard, matchMethod);
            }

            return (scryfallCard, matchMethod);
        }

        private string DetermineSet(CardWriteable c)
        {
            var set = c.SetArena.ToLower();
            if (c.IdArena >= 75890 && c.IdArena <= 75904)
            {
                // European Lands, ArenaSet is ANA but ScryfallSet should be pelp
                set = "pelp";
            }
            else if (dictSetArenaToScryfall.ContainsKey(set) == false)
            {
                if (c.SetArena == "ArenaSUP")
                {
                    switch (c.Name)
                    {
                        case "Plant":
                            set = "bfz";
                            break;

                        case "Zombie Army":
                            set = "war";
                            break;

                        case "Treasure":
                            set = "xln";
                            break;

                        case "Beast":
                            set = "p04";
                            break;
                    }
                }
                else if (set.Length == 4 && set.StartsWith('t'))
                {
                    // When created from scryfall cards that is a token in the set
                    set = set.Substring(1, 3);
                }
                //else
                //    Debugger.Break();
            }
            else if (c.SetArena == "MIR" && (c.Name == "Plains" || c.Name == "Island" || c.Name == "Swamp" || c.Name == "Mountain" || c.Name == "Forest"))
                set = "ana";
            else
                set = dictSetArenaToScryfall[set];

            return set;
        }
    }
}