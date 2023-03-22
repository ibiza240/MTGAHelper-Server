using MTGAHelper.Entity;
using MTGAHelper.Entity.OutputLogParsing;
using MTGAHelper.Entity.UserHistory;
using MTGAHelper.Lib.CardProviders;
using MTGAHelper.Lib.InventoryUpdatedConverters;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MTGAHelper.Lib.UserHistory
{
    public class UserHistorySummaryBuilder
    {
        public const string INVENTORYUPDATECONTEXT_BOOSTEROPEN = "Booster.Open";
        public const string INVENTORYUPDATECONTEXT_CARDSCRAFTED = "PlayerInventory.RedeemBulkWildcards";
        public const string INVENTORYUPDATECONTEXT_BUYCHEST = "Store.Fulfillment.Chest";

        IReadOnlyDictionary<int, Card> allCards;
        IReadOnlyDictionary<int, Set> allSets;
        BoosterOpenConverter boosterOpenConverter;
        CraftedCardsConverter craftedCardsConverter;

        public UserHistorySummaryBuilder(
            ICardRepository cardRepo,
            CacheSingleton<IReadOnlyDictionary<int, Set>> cacheSets,
            BoosterOpenConverter boosterOpenConverter,
            CraftedCardsConverter craftedCardsConverter)
        {
            allCards = cardRepo;
            allSets = cacheSets.Get();
            this.boosterOpenConverter = boosterOpenConverter;
            this.craftedCardsConverter = craftedCardsConverter;
        }

        //public HistorySummaryForDate BuildHistoryForDate(
        //    ICollection<InventoryUpdatedRaw> inventoryUpdates,
        //    ICollection<PostMatchUpdateRaw> postMatchUpdates
        //)
        //{
        //    var changesByContext = new Dictionary<string, HistorySummaryForDate>();

        //    var openedBoosters = inventoryUpdates.Where(i => i.context == INVENTORYUPDATECONTEXT_BOOSTEROPEN)
        //        .Select(i => boosterOpenConverter.Convert(i))
        //        .ToArray();

        //    var cardsCrafted = inventoryUpdates.Where(i => i.context == INVENTORYUPDATECONTEXT_CARDSCRAFTED)
        //        .SelectMany(i => craftedCardsConverter.Convert(i))
        //        .ToArray();

        //    changesByContext.Add(INVENTORYUPDATECONTEXT_BOOSTEROPEN, BuildFromOpenedBoosters(openedBoosters));
        //    changesByContext.Add(INVENTORYUPDATECONTEXT_CARDSCRAFTED, BuildFromCardsCrafted(cardsCrafted));
        //    //changesByContext.Add(INVENTORYUPDATECONTEXT_BUYCHEST, BuildFromMoneyPurchase(cardsCrafted));

        //    var finalResult = BuildFinalResult(changesByContext.Values);
        //    return finalResult;
        //}

        //HistorySummaryForDate BuildFromOpenedBoosters(ICollection<BoosterOpened> openedBoosters)
        //{
        //    var resultBoosters = new HistorySummaryForDate();

        //    resultBoosters.BoostersChange = openedBoosters
        //        .GroupBy(i => i.Set)
        //        .ToDictionary(i => i.Key, i => -i.Count());

        //    resultBoosters.NewCardsCount = openedBoosters.Sum(i => i.Cards.Count(x => x.IsNew));

        //    resultBoosters.GemsChange = openedBoosters.Sum(i => i.Gems);

        //    resultBoosters.WildcardsChange = openedBoosters
        //        .SelectMany(i => i.Cards.Where(x => x.IsWildcard).Select(x => x.Rarity))
        //        .Concat(openedBoosters.Select(i => i.WildcardFromTrack))
        //        .Where(i => i != RarityEnum.Unknown)
        //        .GroupBy(i => i)
        //        .ToDictionary(i => i.Key, i => i.Count());

        //    resultBoosters.VaultProgressChange = openedBoosters.Sum(i => i.VaultProgress);

        //    return resultBoosters;
        //}


        //HistorySummaryForDate BuildFromCardsCrafted(ICollection<Card> craftedCards)
        //{
        //    var resultCraftedCards = new HistorySummaryForDate();

        //    resultCraftedCards.NewCardsCount = craftedCards.Count();

        //    resultCraftedCards.WildcardsChange = craftedCards
        //        .GroupBy(i => i.GetRarityEnum())
        //        .ToDictionary(i => i.Key, i => -i.Count());

        //    return resultCraftedCards;
        //}

        ////private HistorySummaryForDate BuildFromMoneyPurchase(Card[] buyChest)
        ////{
        ////    var resultBuyChest = new HistorySummaryForDate();

        ////    resultBuyChest.BoostersChange

        ////    return resultBuyChest;
        ////}

        ////private HistorySummaryForDate BuildFromInventoryUpdatedRaw(InventoryUpdatedRaw inventoryUpdated)
        ////{
        ////    var result = new HistorySummaryForDate();

        ////    result.BoostersChange = inventoryUpdated.updates.SelectMany(i => i.delta.boosterDelta)
        ////        .ToDictionary(i => allSets[i.collationId].Code, i => i.count);

        ////    result.GemsChange = inventoryUpdated.updates.Sum(i => i.delta.gemsDelta);
        ////    result.GoldChange = inventoryUpdated.updates.Sum(i => i.delta.goldDelta);

        ////    return result;
        ////}

        public HistorySummaryForDate Build(IEnumerable<InventoryUpdatedRaw> inventoryUpdates, ICollection<PostMatchUpdateRaw> postMatchUpdates)
        {
            var i = BuildFromInventoryUpdates(inventoryUpdates);
            var m = BuildFromPostMatchUpdatesMerged(postMatchUpdates);
            return MergeSummaries(new[] { i, m });
        }

        HistorySummaryForDate MergeSummaries(ICollection<HistorySummaryForDate> changesByContext)
        {
            var result = new HistorySummaryForDate();
            result.Contexts = changesByContext.SelectMany(i => i.Contexts).Distinct().ToArray();

            result.BoostersChange = changesByContext
                .Where(i => i.BoostersChange != null)
                .SelectMany(i => i.BoostersChange)
                .GroupBy(kvp => kvp.Key)
                .ToDictionary(g => g.Key, g => g.Select(x => x.Value).Sum());

            //result.ConstructedRank
            //result.Date
            //result.LimitedRank
            //result.OutcomesByMode

            result.GemsChange = changesByContext.Sum(i => i.GemsChange);

            result.GoldChange = changesByContext.Sum(i => i.GoldChange);

            result.NewCardsCount = changesByContext.Sum(i => i.NewCardsCount);

            result.VaultProgressChange = changesByContext.Sum(i => i.VaultProgressChange);

            result.WildcardsChange = changesByContext
                .Where(i => i.WildcardsChange != null)
                .SelectMany(i => i.WildcardsChange)
                .GroupBy(kvp => kvp.Key)
                .ToDictionary(g => g.Key, g => g.Select(x => x.Value).Sum());

            result.NewCards = changesByContext
                .Where(i => i.NewCards != null)
                .SelectMany(i => i.NewCards)
                .GroupBy(kvp => kvp.Key)
                .ToDictionary(g => g.Key, g => g.Select(x => x.Value).Sum());

            result.XpChange = changesByContext.Sum(i => i.XpChange);

            return result;
        }

        public HistorySummaryForDate BuildFromInventoryUpdates(IEnumerable<InventoryUpdatedRaw> inventoryUpdates)
        {
            if (inventoryUpdates == null)
                return new HistorySummaryForDate();

            var result = new HistorySummaryForDate();

            var updates = inventoryUpdates
                .SelectMany(i => i.updates)
                .ToArray();
            var deltas = updates
                .Select(i => i.delta);

            result.BoostersChange = deltas
                .SelectMany(i => i.boosterDelta)
                .GroupBy(i => allSets[i.collationId].Code)
                .ToDictionary(g => g.Key, g => g.Select(x => x.count).Sum());

            //result.ConstructedRank
            //result.Date
            //result.LimitedRank
            //result.OutcomesByMode

            result.GemsChange = deltas.Sum(i => i.gemsDelta);

            result.GoldChange = deltas.Sum(i => i.goldDelta);

            result.NewCardsCount = deltas.Sum(i => i.cardsAdded.Count);
            result.NewCards = deltas.SelectMany(i => i.cardsAdded)
                .GroupBy(i => i)
                .ToDictionary(i => i.Key, i => i.Count());

            result.VaultProgressChange = deltas.Sum(i => i.vaultProgressDelta);

            result.WildcardsChange = new Dictionary<RarityEnum, int>
            {
                {  RarityEnum.Common, deltas.Sum(i => i.wcCommonDelta) },
                {  RarityEnum.Uncommon, deltas.Sum(i => i.wcUncommonDelta) },
                {  RarityEnum.Rare, deltas.Sum(i => i.wcRareDelta) },
                {  RarityEnum.Mythic, deltas.Sum(i => i.wcMythicDelta) },
            };

            result.XpChange = updates.Sum(i => i.xpGained);

            return result;
        }

        public ICollection<HistorySummaryForDate> BuildFromPostMatchUpdates(ICollection<PostMatchUpdateRaw> postMatchUpdates)
        {
            if (postMatchUpdates == null)
                return Array.Empty<HistorySummaryForDate>();

            var questUpdates = postMatchUpdates.Where(i => i.questUpdate != null).SelectMany(i => i.questUpdate).Where(i => i.endingProgress == i.goal);
            var dailyWinUpdates = postMatchUpdates.Where(i => i.dailyWinUpdates != null).SelectMany(i => i.dailyWinUpdates);
            var weeklyWinUpdates = postMatchUpdates.Where(i => i.weeklyWinUpdates != null).SelectMany(i => i.weeklyWinUpdates);
            var battlePassUpdatesRaw = postMatchUpdates.Where(i => i.battlePassUpdate != null).Select(i => i.battlePassUpdate);

            var dailyAndWeeklyWinsUpdates = dailyWinUpdates.Select(i => new InventoryUpdatedRaw
            {
                context = "DailyWin",
                updates = new[] { i },

            }).Union(weeklyWinUpdates.Select(i => new InventoryUpdatedRaw
            {
                context = "WeeklyWin",
                updates = new[] { i },

            }))
            .ToArray();

            var battlePassUpdates = battlePassUpdatesRaw.Where(i => i.trackDiff.oldLevel != i.trackDiff.currentLevel).Select(i => new InventoryUpdatedRaw
            {
                context = $"Battlepass Level {i.trackDiff.currentLevel + 1}",
                updates = i.trackDiff.inventoryUpdates,
            }).ToArray();

            var summaryQuestsUpdates = BuildFromQuestsUpdates(questUpdates);
            if (summaryQuestsUpdates.GoldChange > 0)
                summaryQuestsUpdates.Contexts = new[] { $"{summaryQuestsUpdates.Contexts.Count} QuestComplete" };

            var summaryDailyAndWeeklyWins = BuildFromInventoryUpdates(dailyAndWeeklyWinsUpdates);
            summaryDailyAndWeeklyWins.Contexts = dailyAndWeeklyWinsUpdates.Select(i => i.context).Distinct().ToArray();
            summaryDailyAndWeeklyWins.NewCards = dailyAndWeeklyWinsUpdates.SelectMany(b => b.updates.SelectMany(u => u.delta.cardsAdded))
                .GroupBy(i => i)
                .ToDictionary(i => i.Key, i => i.Count());

            var summaryBattlePassUpdates = BuildFromInventoryUpdates(battlePassUpdates);
            summaryBattlePassUpdates.NewCards = battlePassUpdates.SelectMany(b => b.updates.SelectMany(u => u.delta.cardsAdded))
                .GroupBy(i => i)
                .ToDictionary(i => i.Key, i => i.Count());
            summaryBattlePassUpdates.Contexts = battlePassUpdates.Select(i => i.context).Distinct().ToArray();

            return new[]
            {
                summaryQuestsUpdates,
                summaryDailyAndWeeklyWins,
                summaryBattlePassUpdates,
            };
        }

        public HistorySummaryForDate BuildFromPostMatchUpdatesMerged(ICollection<PostMatchUpdateRaw> postMatchUpdates)
        {
            var merged = MergeSummaries(BuildFromPostMatchUpdates(postMatchUpdates));
            return merged;
        }

        public HistorySummaryForDate BuildFromQuestsUpdates(IEnumerable<QuestUpdate> questUpdates)
        {
            var result = new HistorySummaryForDate();

            var questsCompleted = questUpdates
                .Where(i => i.endingProgress == i.goal)
                .ToArray();

            result.Contexts = questsCompleted.Select(i => "QuestComplete").ToArray();

            result.GoldChange = questsCompleted.Sum(i => string.IsNullOrWhiteSpace(i.chestDescription.quantity) ? 0 : Convert.ToInt32(i.chestDescription.quantity));

            result.XpChange = questsCompleted.Sum(i => 500);

            return result;
        }
    }
}
