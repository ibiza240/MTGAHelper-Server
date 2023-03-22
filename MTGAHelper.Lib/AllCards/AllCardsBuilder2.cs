//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using MTGAHelper.Entity;
//using MTGAHelper.Lib.IO.Reader.MtgaDataCards;
//using MTGAHelper.Lib.IO.Reader.Scryfall;
//using Serilog;

//namespace MTGAHelper.Lib.AllCards
//{
//    public class AllCardsBuilder2
//    {
//        const string CARD_UNKOWN_IMAGE = "https://cdn11.bigcommerce.com/s-0kvv9/images/stencil/1280x1280/products/266486/371622/classicmtgsleeves__43072.1532006814.jpg";

//        ReaderMtgaDataCards2 readerMtgaDataCards;
//        ReaderScryfallCards readerScryfallCards;
//        ICollection<string> bannedCards;

//        ICollection<MtgaDataCardsRootObjectExtended> mtgaCards;
//        ICollection<ScryfallModelRootObject> scryfallCards;
//        Dictionary<string, int> nbCardsBySet;

//        public AllCardsBuilder2(
//            ReaderMtgaDataCards2 readerMtgaDataCards,
//            ReaderScryfallCards readerScryfallCards,
//            BannedCardsProviderTemp bannedCardsProvider)
//        {
//            this.readerMtgaDataCards = readerMtgaDataCards;
//            this.readerScryfallCards = readerScryfallCards;
//            this.bannedCards = bannedCardsProvider.GetBannedCards();
//        }

//        public AllCardsBuilder2 Init(Dictionary<string, int> nbCardsBySet)
//        {
//            //var thbCards = new MtgaToolCardsLoader().LoadTHB();

//            scryfallCards = readerScryfallCards.ReadFile("scryfall-default-cards.json")
//                .Concat(readerScryfallCards.ReadFile("scryfall-scraped.json"))
//                .ToArray();
//            this.nbCardsBySet = nbCardsBySet;

//            mtgaCards = readerMtgaDataCards.GetMtgaCards()
//                //.Where(i => i.set != "THB")
//                //.Union(EnrichMtgaToolCards(thbCards))
//                .ToArray();

//            return this;
//        }

//        public ICollection<Card2> GetFullCards()
//        {
//            Dictionary<int, string> cardFaces = new Dictionary<int, string>();

//            var result = new List<Card2>();

//            foreach (var mtgaCard in mtgaCards
//                .Where(i => i.set != "ArenaSUP"))
//            {
//                //if (mtgaCard.Name == "Thalia, Guardian of Thraben")
//                //    Debugger.Break();

//                try
//                {
//                    //if (mtgaCard.grpid == 70262 || mtgaCard.grpid == 70488) Debugger.Break();
//                    //if (mtgaCard.grpid == 70477) Debugger.Break();

//                    //if (int.TryParse(mtgaCard.CollectorNumber, out int number) == false) Debugger.Break();

//                    var c = new Card2
//                    {
//                        GrpId = mtgaCard.grpid,
//                        Name = mtgaCard.Name,
//                        IsToken = mtgaCard.isToken,
//                        IsCollectible = mtgaCard.isCollectible,
//                        //IsCraftable = mtgaCard.isCraftable,
//                        Power = mtgaCard.power,
//                        Toughness = mtgaCard.toughness,
//                        Number = mtgaCard.CollectorNumber,
//                        Cmc = mtgaCard.cmc,
//                        Rarity = ConvertRarity(mtgaCard.rarity),
//                        Artist = mtgaCard.artistCredit,
//                        Set = ConvertSet(mtgaCard.set),
//                        LinkedFaceType = mtgaCard.linkedFaceType,
//                        Colors = mtgaCard.colors.Select(i => ConvertColor(i)).ToArray(),
//                        Color_identity = mtgaCard.colorIdentity.Select(i => ConvertColor(i)).ToArray(),
//                    };

//                    var scryfallCard = FindCardFromScryfall(c);
//                    if (scryfallCard == null)
//                    {
//                        Log.Information("Card {name} not found in Scryfall", c.Name, c.Set);
//                        c.imageCardUrl = CARD_UNKOWN_IMAGE;
//                        c.imageArtUrl = CARD_UNKOWN_IMAGE;
//                        c.Type_line = "";
//                    }
//                    else
//                    {
//                        c.Type_line = scryfallCard.type_line;

//                        if (c.LinkedFaceType == enumLinkedFace.AdventureCreature || c.LinkedFaceType == enumLinkedFace.AdventureAdventure)
//                        {
//                            if (c.LinkedFaceType == enumLinkedFace.AdventureCreature)
//                            {
//                                c.Mana_cost = scryfallCard.mana_cost.Split("//")[0].Trim();
//                                cardFaces.Add(c.GrpId, scryfallCard.name.Split("//")[1].Trim());
//                            }
//                            else if (c.LinkedFaceType == enumLinkedFace.AdventureAdventure)
//                            {
//                                c.Mana_cost = scryfallCard.mana_cost.Split("//")[1].Trim();
//                                cardFaces.Add(c.GrpId, scryfallCard.name.Split("//")[0].Trim());
//                            }
//                        }
//                        else
//                            c.Mana_cost = scryfallCard.mana_cost;

//                        c.Artist = scryfallCard.artist;

//                        //c.Colors = scryfallCard.colors;
//                        //c.Color_identity = scryfallCard.color_identity;

//                        c.imageCardUrl = GetImageCard(c, scryfallCard);
//                        c.imageArtUrl = GetImageArt(c, scryfallCard);
//                    }

//                    if (IsNotInBooster(c))
//                    {
//                        c.IsInBooster = false;
//                    }

//                    result.Add(c);
//                }
//                catch (Exception ex)
//                {
//                    Debugger.Break();
//                }
//            }

//            foreach (var c in result.Where(i => i.LinkedFaceType == enumLinkedFace.AdventureAdventure || i.LinkedFaceType == enumLinkedFace.AdventureCreature))
//            {
//                c.LinkedCardGrpId = result.Single(i => i.Name == cardFaces[c.GrpId]).GrpId;
//            }

//            return result;
//        }

//        private bool IsNotInBooster(Card2 c)
//        {
//            int.TryParse(c.Number, out int number);

//            var isM20CommonDual = c.Set == "M20" && (
//                c.Name == "Bloodfell Caves" ||
//                c.Name == "Blossoming Sands" ||
//                c.Name == "Dismal Backwater" ||
//                c.Name == "Jungle Hollow" ||
//                c.Name == "Rugged Highlands" ||
//                c.Name == "Scoured Barrens" ||
//                c.Name == "Swiftwater Cliffs" ||
//                c.Name == "Thornwood Falls" ||
//                c.Name == "Tranquil Cove" ||
//                c.Name == "Wind-Scarred Crag"
//            );

//            var t = number == 0 ||
//                bannedCards.Contains(c.Name) ||
//                //c.IsCraftable == false ||
//                nbCardsBySet.ContainsKey(c.Set) == false ||
//                number > nbCardsBySet[c.Set] ||
//                c.LinkedFaceType == enumLinkedFace.AdventureAdventure ||
//                c.LinkedFaceType == enumLinkedFace.SplitCard ||
//                isM20CommonDual;

//            return t;
//        }

//        private string ConvertSet(string set)
//        {
//            return set == "MI" ? "MIR" : set == "PS" ? "PLS" : set == "DAR" ? "DOM" : set;
//        }

//        private string ConvertColor(int i)
//        {
//            switch (i)
//            {
//                case 1: return "W";
//                case 2: return "U";
//                case 3: return "B";
//                case 4: return "R";
//                case 5: return "G";
//            }
//            throw new NotSupportedException("Color not supported");
//        }

//        private string GetImageArt(Card2 c, ScryfallModelRootObject scryfallCard)
//        {
//            //CreateMap<MtgaDataCardsRootObject, Card>()
//            //    .ForMember(i => i.set, i => i.MapFrom(x => x.set == "MI" ? "MIR" : x.set == "PS" ? "PLS" : x.set.ToUpper().Replace("DAR", "DOM")))
//            //    .ForMember(i => i.number, i => i.MapFrom(x => x.CollectorNumber));

//            //CreateMap<ScryfallModelRootObject, Card>()
//            //    .ForMember(i => i.name, i => i.MapFrom(x => x.layout == "transform" ? x.name.Split('/')[0].Trim() : x.name))
//            //    .ForMember(i => i.grpId, i => i.MapFrom(x => x.arena_id))
//            //    .ForMember(i => i.set, i => i.MapFrom(x => x.set.ToUpper().Replace("DAR", "DOM")))
//            //    .ForMember(i => i.number, i => i.MapFrom(x => x.collector_number))
//            //    .ForMember(i => i.type, i => i.MapFrom(x => x.layout == "transform" ? x.type_line.Split('/')[0].Trim() : x.type_line))
//            //    .ForMember(i => i.imageCardUrl, i => i.MapFrom(x => (x.layout == "transform" ? x.card_faces[0].image_uris.border_crop : x.image_uris.border_crop).Replace("https://img.scryfall.com/cards", "")))
//            //    .ForMember(i => i.imageArtUrl, i => i.MapFrom(x => (x.layout == "transform" ? x.card_faces[0].image_uris.art_crop : x.image_uris.art_crop).Replace("https://img.scryfall.com/cards", "")))
//            //    .ForMember(i => i.colors, i => i.MapFrom(x => x.color_identity.Select(y => y.ToUpper())));

//            if (scryfallCard.name == c.Name || scryfallCard.card_faces == null)
//                return scryfallCard.image_uris.art_crop;
//            else
//            {
//                try
//                {
//                    if (scryfallCard.name.IndexOf(c.Name) == 0)
//                        return scryfallCard.card_faces[0]?.image_uris?.art_crop ?? scryfallCard.image_uris.art_crop;
//                    else
//                        return scryfallCard.card_faces[1]?.image_uris?.art_crop ?? scryfallCard.image_uris.art_crop;
//                }
//                catch (Exception ex)
//                {
//                    Debugger.Break();
//                }
//            }

//            return null;
//        }

//        private string GetImageCard(Card2 c, ScryfallModelRootObject scryfallCard)
//        {
//            if (scryfallCard.name == c.Name || scryfallCard.card_faces == null)
//                return scryfallCard.image_uris.border_crop;
//            else
//            {
//                try
//                {
//                    if (scryfallCard.name.IndexOf(c.Name) == 0)
//                        return scryfallCard.card_faces[0]?.image_uris?.border_crop ?? scryfallCard.image_uris.border_crop;
//                    else
//                        return scryfallCard.card_faces[1]?.image_uris?.border_crop ?? scryfallCard.image_uris.border_crop;
//                }
//                catch (Exception ex)
//                {
//                    Debugger.Break();
//                }
//            }

//            return null;
//        }

//        private ScryfallModelRootObject FindCardFromScryfall(Card2 c)
//        {
//            var scryfallCard = scryfallCards.FirstOrDefault(i => i.name == c.Name && i.set.ToUpper() == c.Set && i.collector_number == c.Number.ToString());

//            if (scryfallCard == null)
//                scryfallCard = scryfallCards.FirstOrDefault(i => i.name == c.Name && i.set.ToUpper() == c.Set && i.artist == c.Artist);

//            // Cards that flip over
//            if (scryfallCard == null)
//                scryfallCard = scryfallCards.FirstOrDefault(i => i.name.StartsWith($"{c.Name} //") || i.name.EndsWith($"// {c.Name}") && i.set.ToUpper() == c.Set && i.artist.Contains(c.Artist));

//            // Tokens
//            if (scryfallCard == null)
//                scryfallCard = scryfallCards.FirstOrDefault(i => i.name == c.Name && i.set.ToUpper() == $"T{c.Set}");

//            // Mythic edition (Like Vraska, Ral, ...)
//            if (scryfallCard == null)
//                scryfallCard = scryfallCards.FirstOrDefault(i => i.name == c.Name && i.set == "med" && i.artist == c.Artist);

//            // ANA only, pick an alternative
//            if (scryfallCard == null && c.Set == "ANA")
//                scryfallCard = scryfallCards.FirstOrDefault(i => i.name == c.Name && i.artist == c.Artist);

//            return scryfallCard;
//        }

//        string ConvertRarity(int r)
//        {
//            switch (r)
//            {
//                case 0: // Token
//                case 1: // Basic Land
//                case 2:
//                    return "Common";
//                case 3:
//                    return "Uncommon";
//                case 4:
//                    return "Rare";
//                case 5:
//                    return "Mythic";
//            }

//            throw new NotSupportedException("Invalid rarity");
//        }
//    }
//}