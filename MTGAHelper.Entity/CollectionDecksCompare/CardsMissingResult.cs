﻿using System.Collections.Generic;
using System.Linq;

namespace MTGAHelper.Entity.CollectionDecksCompare
{
    public class CardsMissingResult
    {
        readonly ICollection<CardRequiredInfo> computedData;

        public Dictionary<string, CardRequiredInfoByDeck> ByDeck { get; private set; } = new Dictionary<string, CardRequiredInfoByDeck>();

        public Dictionary<string, CardRequiredInfoByCard> ByCard { get; set; } = new Dictionary<string, CardRequiredInfoByCard>();

        public CardsMissingResult()
        {
        }

        public CardsMissingResult(ICollection<CardRequiredInfo> computedData, Dictionary<string, bool> decksTracked)
        {
            //var test = computedData.FirstOrDefault(i => i.Card.name == "Teferi, Hero of Dominaria");
            this.computedData = computedData;

            ByDeck = computedData
                .Where(i => i.IsForAverageArchetypeOthersInMain == false)
                .GroupBy(i => i.DeckId)
                .ToDictionary(i => i.Key, i => new CardRequiredInfoByDeck(i.Key, i, decksTracked));

            ByCard = computedData
                .Where(i => i.IsForAverageArchetypeOthersInMain == false)
                .GroupBy(i => i.Card.Name)
                .ToDictionary(i => i.Key, i => new CardRequiredInfoByCard(i, decksTracked));

            //var test2 = ByCard.FirstOrDefault(i => i.Key.name == "Teferi, Hero of Dominaria");
        }

        public Dictionary<string, InfoCardMissingSummary[]> GetModelSummary(bool boostersOnly = false)
        {
            var data = ByCard
                .Where(i => boostersOnly == false || (i.Value.Card.IsFoundInBooster && i.Value.Card.Set != "JMP"))
                .Where(i => i.Value.NbMissing > 0)
                ;

            //var cards = data
            //    .Select(i => new { i.Value.Card, i.Value.MissingWeight, i.Value.NbMissing })
            //    .Where(i => i.Card.set == "ZNR")
            //    .OrderByDescending(i => i.MissingWeight)
            //    .ToArray();
            //var test = JsonConvert.SerializeObject(cards);

            var ret = data
                .GroupBy(i =>
                //// C# 9
                //i.Value.Card.set switch {
                //    "STA" or "STX" => "STX",
                //    _ => i.Value.Card.set
                //}
                i.Value.Card.Set == "STA" || i.Value.Card.Set == "STX" ? "STX" : i.Value.Card.Set
                )
                .OrderByDescending(i => i.Sum(x => x.Value.MissingWeight))
                .ToDictionary(i => i.Key, x => x
                    .OrderBy(i => i.Value.Card.GetRarityEnumSplitRareLands())
                    .GroupBy(i => i.Value.Card.GetRarityEnumSplitRareLands()).Select(i => new InfoCardMissingSummary
                    {
                        Set = x.Key,
                        //NotInBooster = x.Key == Card.NOTINBOOSTER,
                        Rarity = i.Key,
                        NbMissing = i.Sum(c => c.Value.NbMissing),
                        MissingWeight = i.Sum(c => c.Value.MissingWeight)
                    }).ToArray());

            return ret;
        }

        public CardMissingDetailsModel[] GetModelDetails()
        {
            var ret = ByCard
                //.Where(i => i.Value.NbMissing > 0)
                //.Where(i =>/* i.Value.Card.type.Contains("Land") ||*/ i.Value.MissingWeight != 0)
                .Select(i => new CardMissingDetailsModel
                {
                    CardName = i.Value.Card.Name,
                    Set = i.Value.Card.Set,
                    //SetId = i.Key.number,
                    NotInBooster = i.Value.Card.NotInBooster,
                    ImageCardUrl = i.Value.Card.ImageCardUrl,//i.Key.images["normal"],
                    Rarity = i.Value.Card.GetRarityEnumSplitRareLands(),
                    Type = i.Value.Card.Type,
                    NbOwned = i.Value.NbOwned,
                    NbMissing = i.Value.NbMissing,
                    MissingWeight = i.Value.MissingWeight,
                    NbDecks = i.Value.NbDecks,
                    NbAvgPerDeck = System.Math.Round((double)i.Value.ByDeck.Sum(x => x.Value.NbRequired) / i.Value.ByDeck.Count, 2),
                })
                .OrderByDescending(i => i.MissingWeight)
                .ThenBy(i => i.CardName)
                .ToArray();
            return ret;
        }

        public InfoCardInDeck[] GetModelMissingCardsAllDecks()
        {
            return computedData
                //.Where(i => i.IsForAverageArchetypeOthersInMain == false)
                .Where(i => i.NbMissing > 0)
                .GroupBy(i => new { i.DeckName, i.Card })
                .Select(i => new InfoCardInDeck
                {
                    Set = i.Key.Card.Set,
                    CardName = i.Key.Card.Name,
                    Rarity = i.Key.Card.RarityStr,
                    Type = i.Key.Card.Type,
                    Deck = i.Key.DeckName,
                    NbMissingMain = i.Where(x => x.IsSideboard == false).Sum(x => x.NbMissing),
                    NbMissingSideboard = i.Where(x => x.IsSideboard).Sum(x => x.NbMissing),
                })
                .OrderBy(i => i.CardName)
                .ThenBy(i => i.Deck)
                .ToArray();
        }

        public Dictionary<RarityEnum, int> GetWildcardsMissing(string deckId, bool isSideboard, bool splitRareLands)
        {
            if (ByDeck.ContainsKey(deckId) == false)
            {
                //Log.Error
                return new Dictionary<RarityEnum, int>();
            }

            var ret = new Dictionary<RarityEnum, int>();
            var cards = ByDeck[deckId].ByCard;

            ret.Add(RarityEnum.Mythic,
                cards
                    .Where(i => i.Value.Card.Rarity == RarityEnum.Mythic)
                    .Sum(i => isSideboard ? i.Value.NbMissingSideboard : i.Value.NbMissingMain));

            var rares = cards.Where(i => i.Value.Card.Rarity == RarityEnum.Rare);
            if (splitRareLands)
            {
                ret.Add(RarityEnum.RareNonLand,
                    rares
                        .Where(i => i.Value.Card.Type.Contains("Land") == false)
                        .Sum(i => isSideboard ? i.Value.NbMissingSideboard : i.Value.NbMissingMain));

                ret.Add(RarityEnum.RareLand,
                    rares
                        .Where(i => i.Value.Card.Type.Contains("Land") == true)
                        .Sum(i => isSideboard ? i.Value.NbMissingSideboard : i.Value.NbMissingMain));
            }
            else
            {
                ret.Add(RarityEnum.Rare,
                    rares.Sum(i => isSideboard ? i.Value.NbMissingSideboard : i.Value.NbMissingMain));
            }

            ret.Add(RarityEnum.Uncommon,
                cards
                    .Where(i => i.Value.Card.Rarity == RarityEnum.Uncommon)
                    .Sum(i => isSideboard ? i.Value.NbMissingSideboard : i.Value.NbMissingMain));

            ret.Add(RarityEnum.Common,
                cards
                    .Where(i => i.Value.Card.Rarity == RarityEnum.Common)
                    .Sum(i => isSideboard ? i.Value.NbMissingSideboard : i.Value.NbMissingMain));

            return ret;
        }
    }
}
