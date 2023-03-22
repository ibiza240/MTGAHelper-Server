using AutoMapper;
using MTGAHelper.Entity;
using MTGAHelper.Entity.MtgaOutputLog;
using MTGAHelper.Lib.MasteryPass;
using MTGAHelper.Lib.UserHistory;
using MTGAHelper.Server.DataAccess;
using MTGAHelper.Server.DataAccess.Queries;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MTGAHelper.Lib.MtgaDeckStats
{
    public class MtgaDeckLimitedResultsBuilder
    {
        private readonly IMapper mapper;
        private readonly StatsLimitedRepository statsLimitedRepository;
        private readonly IQueryHandler<MatchesWithDecksQuery, IReadOnlyList<MatchResult>> qMatches;

        public MtgaDeckLimitedResultsBuilder(
            IMapper mapper,
            StatsLimitedRepository statsLimitedRepository,
            IQueryHandler<MatchesWithDecksQuery, IReadOnlyList<MatchResult>> qMatches
            )
        {
            this.mapper = mapper;
            this.statsLimitedRepository = statsLimitedRepository;
            this.qMatches = qMatches;
        }

        public async Task<Dictionary<string, ICollection<LimitedEventResults>>> GetLimitedResults(string userId, string period)
        {
            var storedStats = await statsLimitedRepository.GetStatsLimited(userId);
            //if (storedStats == null || storedStats.IsUpToDate == false)
            {
                // Rebuild stats
                var currentSet = "ONE";
                var lastSet = "BRO";
                var minDate = period == "currentset" ? SetStartingDates.DictStartingDate[currentSet] :
                    period == "currentandpreviousset" ? SetStartingDates.DictStartingDate[lastSet] : new DateTime(2019, 9, 26);

                var q = new MatchesWithDecksQuery(userId, null, minDate);
                var matches = await qMatches.Handle(q);

                //var json = JsonConvert.SerializeObject(matches);

                try
                {
                    var results = new[] {
                    "Draft",
                    "Sealed",
                }.ToDictionary(i => i, i => GetLimitedResultsFor(userId, matches, i));

                    storedStats = new CosmosDataStatsLimited
                    {
                        IsUpToDate = true,
                        Stats = results,
                    };

                    // Save fresh stats
                    await statsLimitedRepository.SetStatsLimited(userId, storedStats);
                }
                catch (Exception)
                {
                    Log.Information("{userId} GetLimitedResults", userId);
                    throw;
                }
            }

            return storedStats.Stats;
        }

        private ICollection<LimitedEventResults> GetLimitedResultsFor(string userId, IReadOnlyList<MatchResult> matchesRaw, string filter)
        {
            var regex = new Regex("(.*?)_([A-Z0-9]+)_[0-9]+");

            var matches = matchesRaw
                .Where(i => i.EventName?.Contains("CubeDraft", StringComparison.InvariantCultureIgnoreCase) == false)
                .Where(i => i.EventName?.Contains(filter, StringComparison.InvariantCultureIgnoreCase) == true)
                .Where(i => i.DeckUsed != default)
                .Where(i => i.SecondsCount > 0)
                .ToArray();

            // FIX to assign missing deck ids
            var missingDecks = matches.Where(i => i.DeckUsed.Id == default || i.DeckUsed.Id == Guid.Empty.ToString());

            foreach (var m in missingDecks)
            {
                // Recalculate ids with cards all in mainboard for grouping
                var cards = mapper.Map<ICollection<DeckCard>>(m.DeckUsed.Cards)
                    .GroupBy(i => i.Card.Name)
                    .Select(i => new DeckCard(i.First().Card, i.Sum(i => i.Amount), DeckCardZoneEnum.Deck));

                m.DeckUsed.Id = new Deck("", null, cards).GetId();
            }

            var draftResults = matches
                .GroupBy(i => (i.EventName ?? "N/A", i.EventInstanceId ?? i.DeckUsed.Id))
                .Select(i =>
                {
                    var regexMatch = regex.Match(i.Key.Item1);
                    if (regexMatch.Success == false)
                    {
                        Log.Error("{userId} GetLimitedResultsFor missing {eventName}, {deckId}", userId, i.Key.Item1, i.Key.Item2 ?? "NULL");
                        return null;
                    }

                    //var setAndEventType = i.Key.EventName.Split("_");
                    var setAndEventType = new[] { regexMatch.Groups[1].Value, regexMatch.Groups[2].Value };

                    var idxSplit = setAndEventType[0].IndexOf(filter);

                    return new LimitedEventResults
                    {
                        DraftType = setAndEventType[0].Substring(0, idxSplit) + " " + setAndEventType[0].Substring(idxSplit),
                        Set = setAndEventType[1],
                        DateStart = i.Min(m => m.StartDateTime),
                        DateEnd = i.Max(m => m.StartDateTime.AddSeconds(m.SecondsCount)),
                        WinCount = i.Count(m => m.Outcome == GameOutcomeEnum.Victory),
                        LossCount = i.Count(m => m.Outcome == GameOutcomeEnum.Defeat || m.Outcome == GameOutcomeEnum.Unknown),
                        DrawCount = i.Count(m => m.Outcome == GameOutcomeEnum.Draw),
                    };
                })
                .Where(i => i != null)
                .OrderByDescending(i => i.DateStart)
                .ToArray();

            return draftResults;
        }
    }
}