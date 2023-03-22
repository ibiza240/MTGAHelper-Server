using MTGAHelper.Entity;
using MTGAHelper.Entity.UserHistory;
using MTGAHelper.Lib.CardProviders;
using MTGAHelper.Lib.MasteryPass;
using MTGAHelper.Lib.UserHistory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MTGAHelper.Lib
{
    public class EconomicItem
    {
        public DateTime Timestamp { get; set; }
        public string Context { get; set; }
        public EconomyEventChange Changes { get; set; }
    }

    public class EconomyReportGenerator
    {
        private readonly UserHistoryLoader userHistoryLoader;
        private readonly IReadOnlyDictionary<int, Card> allCards;

        public EconomyReportGenerator(
            ICardRepository cardRepo,
            UserHistoryLoader userHistoryLoader
            )
        {
            this.userHistoryLoader = userHistoryLoader;
            this.allCards = cardRepo;
        }

        public string UserId { get; set; }

        public async Task<EconomyResponse> GetResponseEconomy()
        {
            if (UserId == default)
                throw new InvalidOperationException($"You must set the {nameof(UserId)} before calling {nameof(GetResponseEconomy)}");

            //// Get the last 3 months of input data
            //var dateStart = DateTime.Now.AddDays(-90);
            var dateStart = DateTime.MinValue;

            var dateFrom = new DateTime(dateStart.Year, dateStart.Month, 1);
            var raw = await userHistoryLoader.LoadEconomyEventsFromDate(UserId, dateFrom);

            //var json = JsonConvert.SerializeObject(raw);
            //var test = raw.Where(i => i.Changes.Any(x => x.Type == EconomyEventChangeEnum.NewCards)).ToArray();

            var data = raw
                //.SelectMany(i => i.Changes.Select(c => new EconomicItem { Timestamp = i.DateTime, Context = i.Context, Changes = c, NewCards = i.NewCards }))
                .SelectMany(i =>
                    i.Changes
                        // Filter out Changes for cards since they are added just below from the NewCards structure
                        .Where(x => x.Type != EconomyEventChangeEnum.NewCards)
                        .Select(c => new EconomicItem { Timestamp = i.DateTime, Context = i.Context, Changes = c })
                    // Add the new cards as a compatible structure with the Changes
                    .Union(i.NewCards
                        .Where(c => allCards.ContainsKey(c.Key))
                        .Select(c => new EconomicItem
                        {
                            Timestamp = i.DateTime,
                            // We need to distinguish rarities so we add it to the key
                            Context = i.Context + " " + allCards[c.Key].Rarity,
                            Changes = new EconomyEventChange
                            {
                                Type = EconomyEventChangeEnum.NewCards,
                                Amount = c.Value,
                                Value = allCards[c.Key].Rarity.ToString(),
                            }
                        })
                    )
                )
                // Most recent data first
                .OrderByDescending(i => i.Timestamp)
                .ToArray();

            // Add the Set (Booster) or Rarity (Wildcard) to the key of entries of type Booster or Wildcard
            foreach (var d in data)
            {
                if (d.Changes.Type == EconomyEventChangeEnum.Booster || d.Changes.Type == EconomyEventChangeEnum.Wildcard)
                    d.Context += $" {d.Changes.Value}";
            }

            //var test = data
            //    .Where(i => i.Context.Contains("Battlepass"))
            //    .Select(i => (i.Timestamp, i.Context))
            //    .ToArray();

            var dataBySet = data

                //// First group data by month
                //.GroupBy(i => $"{i.Timestamp.Year}-{i.Timestamp.Month:00}-01")
                .GroupBy(i => FindSet(i.Timestamp))

                .ToDictionary(i => i.Key, i => i
                    // Always show the sections in the same order
                    .OrderBy(i => i.Changes.Type switch
                    {
                        EconomyEventChangeEnum.Gems => 0,
                        EconomyEventChangeEnum.Gold => 1,
                        EconomyEventChangeEnum.Xp => 2,
                        EconomyEventChangeEnum.Vault => 3,
                        EconomyEventChangeEnum.NewCards => 4,
                        EconomyEventChangeEnum.Booster => 5,
                        EconomyEventChangeEnum.Wildcard => 6,
                        _ => 99,
                    })
                    .ThenBy(i =>
                        i.Changes.Value == "Mythic" ? 0 :
                        i.Changes.Value == "Rare" ? 1 :
                        i.Changes.Value == "Uncommon" ? 2 :
                        i.Changes.Value == "Common" ? 3 :
                        99
                    )
                    // Then group data by currency Type (Gold, Gems, etc.)
                    .GroupBy(x => x.Changes.Type)
                    .ToDictionary(x => x.Key, x =>
                    {
                        // Compute positive and negative fluctuations distinctively
                        return new EconomyResponseSummary
                        {
                            Gains = ComputeEconomy(x.Key, x.Where(z => z.Changes.Amount > 0)),
                            Spendings = ComputeEconomy(x.Key, x.Where(z => z.Changes.Amount < 0)),
                        };
                    })
                );

            return new EconomyResponse
            {
                DataBySetThenInventoryType = dataBySet
            };
        }

        private string FindSet(DateTime timestamp)
        {
            return SetStartingDates.DictStartingDate.Last(i => i.Value < timestamp).Key;
        }

        private ICollection<EconomyResponseItem> ComputeEconomy(EconomyEventChangeEnum itemType, IEnumerable<EconomicItem> items)
        {
            return items
                // Sum the values by context / key
                .GroupBy(g => g.Context)
                .Select(g => new EconomyResponseItem
                {
                    InventoryItemType = itemType.ToString(),
                    Context = g.Key,
                    Variation = g.Sum(v => v.Changes.Amount),
                    Value = g.First().Changes.Value,
                })
                // Apply a special grouping to band together some messages (e.g. all Battlepass levels, etc.)
                .GroupBy(i =>
                    i.Context.Contains("Battlepass Level") ? "Battlepass Level " + i.Value :
                    i.Context.Contains("QuestComplete") ? "Quest completed" :
                    i.Context.Contains("RedeemBulkWildcards") && i.InventoryItemType == "Wildcard" ? "Redeem bulk wildcards " + i.Value :
                    i.Context.Contains("RedeemBulkWildcards") && i.InventoryItemType == "NewCards" ? "Redeem bulk wildcards " + i.Value :
                    i.Context.Contains("Booster.Open") && i.InventoryItemType == "Booster" ? "Booster open " + i.Value :
                    i.Context.Contains("Booster.Open") && i.InventoryItemType == "Wildcard" ? "Booster open " + i.Value + " WC" :
                    i.Context.Contains("Booster.Open") && i.InventoryItemType == "NewCards" ? "Booster open " + i.Value + " card" :
                    i.Context.StartsWith("FNM_") ? "FNM" :
                    i.Context.Contains("DailyWin") || i.Context.Contains("Weekly") ? "Daily / Weekly wins" :
                    i.Context
                )
                // Build the final output row data with either a one-to-one input row or by summing grouped values
                .Select(i => i.Count() == 1 && i.First().Context == i.Key ? i.Single() :
                    new EconomyResponseItem
                    {
                        InventoryItemType = itemType.ToString(),
                        Context = i.Key,
                        Variation = (float)i.Sum(v => Convert.ToSingle(v.Variation)),
                        Value = i.First().Value,
                    }
                )
                .ToArray();
        }
    }
}