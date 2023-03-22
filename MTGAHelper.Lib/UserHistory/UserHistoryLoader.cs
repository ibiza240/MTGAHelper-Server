using AutoMapper;
using MTGAHelper.Entity;
using MTGAHelper.Entity.MtgaOutputLog;
using MTGAHelper.Entity.OutputLogParsing;
using MTGAHelper.Entity.UserHistory;
using MTGAHelper.Server.DataAccess;
using MTGAHelper.Server.DataAccess.CacheUserHistory;
using MTGAHelper.Server.DataAccess.Queries;
using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace MTGAHelper.Lib.UserHistory
{
    public class UserHistoryLoader
    {
        private readonly DateTime dateNewHistory = new DateTime(2019, 11, 18);

        private readonly IMapper mapper;
        private readonly CacheDataGlobal<GetSeasonAndRankDetailRaw> cacheDataGlobalSeasonAndRankDetail;
        private readonly UserHistorySummaryBuilder userHistorySummaryBuilder;
        private readonly UserHistoryDatesAvailable userHistoryDatesAvailable;
        private readonly RankDeltaCalculator rankDeltaCalculator;

        //private readonly IQueryHandler<RankUpdatesOnDayQuery, InfoByDate<Dictionary<DateTime, RankUpdatedRaw>>> qRankUpdatesOnDay;
        private readonly IQueryHandler<InventoryUpdatesOnDayQuery, InfoByDate<Dictionary<DateTime, InventoryUpdatedRaw>>> qInventoryUpdatesOnDay;

        private readonly IQueryHandler<PostMatchUpdatesOnDayQuery, IReadOnlyDictionary<DateTime, PostMatchUpdateRaw>> qPostMatchUpdatesOnDay;
        private readonly IQueryHandler<MatchesOnDayQuery, IReadOnlyList<MatchResult>> qMatchesOnDay;
        private readonly IQueryHandler<DiffQuery, DateSnapshotDiff> qDiff;
        private readonly IQueryHandler<InventoryUpdatesAfterQuery, IEnumerable<(DateTime, InventoryUpdatedRaw)>> qInventoryUpdatesAfter;
        private readonly IQueryHandler<PostMatchUpdatesAfterQuery, IReadOnlyCollection<KeyValuePair<DateTime, PostMatchUpdateRaw>>> qPostMatchUpdatesAfter;

        public UserHistoryLoader(
            IMapper mapper,
            CacheDataGlobal<GetSeasonAndRankDetailRaw> cacheDataGlobalSeasonAndRankDetail,
            UserHistorySummaryBuilder userHistorySummaryBuilder,
            UserHistoryDatesAvailable userHistoryDatesAvailable,
            RankDeltaCalculator rankDeltaCalculator,
            //IQueryHandler<RankUpdatesOnDayQuery, InfoByDate<Dictionary<DateTime, RankUpdatedRaw>>> qRankUpdatesOnDay,
            IQueryHandler<InventoryUpdatesOnDayQuery, InfoByDate<Dictionary<DateTime, InventoryUpdatedRaw>>> qInventoryUpdatesOnDay,
            IQueryHandler<PostMatchUpdatesOnDayQuery, IReadOnlyDictionary<DateTime, PostMatchUpdateRaw>> qPostMatchUpdatesOnDay,
            IQueryHandler<MatchesOnDayQuery, IReadOnlyList<MatchResult>> qMatchesOnDay,
            IQueryHandler<DiffQuery, DateSnapshotDiff> qDiff,
            IQueryHandler<InventoryUpdatesAfterQuery, IEnumerable<(DateTime, InventoryUpdatedRaw)>> qInventoryUpdatesAfter,
            IQueryHandler<PostMatchUpdatesAfterQuery, IReadOnlyCollection<KeyValuePair<DateTime, PostMatchUpdateRaw>>> qPostMatchUpdatesAfter)
        {
            this.mapper = mapper;
            //this.globalDataLoaderFromFile = globalDataLoaderFromFile;
            this.cacheDataGlobalSeasonAndRankDetail = cacheDataGlobalSeasonAndRankDetail;
            this.userHistorySummaryBuilder = userHistorySummaryBuilder;
            this.userHistoryDatesAvailable = userHistoryDatesAvailable;
            this.rankDeltaCalculator = rankDeltaCalculator;
            //this.qRankUpdatesOnDay = qRankUpdatesOnDay;
            this.qInventoryUpdatesOnDay = qInventoryUpdatesOnDay;
            this.qPostMatchUpdatesOnDay = qPostMatchUpdatesOnDay;
            this.qMatchesOnDay = qMatchesOnDay;
            this.qDiff = qDiff;
            this.qInventoryUpdatesAfter = qInventoryUpdatesAfter;
            this.qPostMatchUpdatesAfter = qPostMatchUpdatesAfter;
        }

        //public ICollection<HistorySummaryForDate> Load(string userId, DateTime dateFrom, DateTime dateTo)
        //{
        //    var summary = new List<HistorySummaryForDate>();

        //    var dt = dateFrom.Date;
        //    while (dt <= dateTo)
        //    {
        //        var inventoryUpdates = LoadInventoryUpdatesForDate(userId, dt);
        //        var summaryForDate = userHistorySummaryBuilder.BuildFromInventoryUpdates(inventoryUpdates);
        //        summaryForDate.Date = dt;

        //        summary.Add(summaryForDate);
        //        dt = dt.AddDays(1);
        //    }

        //    return summary;
        //}

        public async Task<(ICollection<HistorySummaryForDate>, int totalItems, ICollection<string> datesAvailable)> LoadSummary(string userId, int currentPage, int perPage)
        {
            var summary = new List<HistorySummaryForDate>();

            var allDates = await userHistoryDatesAvailable.GetDatesRecentFirst(userId);

            var datesAvailable = allDates
                .Skip(currentPage * perPage)
                .Take(perPage)
                .ToArray();

            foreach (var dateString in datesAvailable)
            {
                var date = DateTime.ParseExact(dateString, "yyyyMMdd", CultureInfo.InvariantCulture);
                var inventoryUpdates = (await qInventoryUpdatesOnDay.Handle(new InventoryUpdatesOnDayQuery(userId, date))).Info.Select(v => v.Value);
                var postMatchUpdates = (await qPostMatchUpdatesOnDay.Handle(new PostMatchUpdatesOnDayQuery(userId, date))).Values.ToArray();

                var summaryForDate = userHistorySummaryBuilder.Build(inventoryUpdates, postMatchUpdates);
                summaryForDate.Date = date;

                //var matchesData = userHistoryLoaderFromFile.LoadMatchesForDate(userId, dateString);
                //var matchesData = await cacheUserHistoryMatches.Get(userId, dateString);
                var matchesData = await qMatchesOnDay.Handle(new MatchesOnDayQuery(userId, date));

                summaryForDate.OutcomesByMode = matchesData
                    .Where(i => i.Opponent.ScreenName != "Sparky")
                    //.Where(i => i.EventName != null)
                    .GroupBy(i => i.EventName ?? "Unknown")
                    .ToDictionary(i => i.Key, i => new Outcomes
                    {
                        Wins = i.Count(x => x.Outcome == GameOutcomeEnum.Victory),
                        Losses = i.Count(x => x.Outcome == GameOutcomeEnum.Defeat),
                    });

                //var (constructedRankChange, limitedRankChange) = await GetRanks(userId, dateString);
                //summaryForDate.ConstructedRankChange = constructedRankChange;
                //summaryForDate.LimitedRankChange = limitedRankChange;

                summary.Add(summaryForDate);
            }

            return (summary, allDates.Count, datesAvailable);
        }

        //private Task<RankDelta> GetDeltaFromRankUpdatedSingle(DateTime dateTime, RankUpdatedRaw raw) =>
        //    GetDeltaFromRankUpdatedList(new Dictionary<DateTime, RankUpdatedRaw> { { dateTime, raw } }, (RankFormatEnum)Enum.Parse(typeof(RankFormatEnum), raw.rankUpdateType));

        //private async Task<RankDelta> GetDeltaFromRankUpdatedList(IReadOnlyDictionary<DateTime, RankUpdatedRaw> ranksData, RankFormatEnum format)
        //{
        //    RankDelta rankChange = new RankDelta();
        //    var rankChangesRaw = ranksData.Where(i => i.Value.rankUpdateType == format.ToString()).OrderBy(i => i.Key).ToArray();
        //    if (rankChangesRaw.Any())
        //    {
        //        var first = rankChangesRaw.First();
        //        var fv = first.Value;

        //        var rankChanges = new[]
        //        {
        //                    new Rank
        //                    {
        //                        SeasonOrdinal = fv.seasonOrdinal,
        //                        Format = (RankFormatEnum)Enum.Parse(typeof(RankFormatEnum), fv.rankUpdateType),
        //                        Class = fv.oldClass,
        //                        Level = fv.oldLevel,
        //                        Step = fv.oldStep,
        //                    }
        //                }
        //            .Union(mapper.Map<ICollection<Rank>>(rankChangesRaw.Select(i => i.Value)))
        //            .ToArray();

        //        //var seasonConfig = globalDataLoaderFromFile.GetSeasonConfig(ranksData.Last().Value.seasonOrdinal);
        //        var seasonConfig = await cacheDataGlobalSeasonAndRankDetail.GetData($"seasons/GetSeasonAndRankDetailRaw{ranksData.Last().Value.seasonOrdinal}.json");
        //        if (seasonConfig?.constructedRankInfo == null)
        //            Log.Error("Cannot find SeasonConfig for ordinal {seasonOrdinal}!", ranksData.Last().Value.seasonOrdinal);
        //        else
        //            rankChange = rankDeltaCalculator.GetDelta(first.Key, format, rankChanges.First(), rankChanges.Last(), seasonConfig);
        //    }

        //    return rankChange;
        //}

        //private async Task<(RankDelta constructedRankChange, RankDelta limitedRankChange)> GetRanks(string userId, string dateString)
        //{
        //    //var ranksData = userHistoryLoaderFromFile.LoadRankUpdatedForDate(userId, dateString);
        //    var ranksData = (await qRankUpdatesOnDay.Handle(new RankUpdatesOnDayQuery(userId, dateString))).Info;

        //    if (!ranksData.Any())
        //        return (new RankDelta(), new RankDelta());

        //    var constructedRankChange = await GetDeltaFromRankUpdatedList(ranksData, RankFormatEnum.Constructed);
        //    var limitedRankChange = await GetDeltaFromRankUpdatedList(ranksData, RankFormatEnum.Limited);

        //    return (constructedRankChange, limitedRankChange);
        //}

        private EconomyEvent ConvertFromRaw(string context, DateTime timestamp, HistorySummaryForDate result)
        {
            return new EconomyEvent
            {
                Context = context,
                DateTime = timestamp,
                Changes = result.DescriptionList,
                NewCards = result.NewCards,
            };
        }

        //public async Task<ICollection<RankDelta>> LoadRankUpdatesForDate(string userId, DateTime dateFor)
        //{
        //    //var ranksData = userHistoryLoaderFromFile.LoadRankUpdatedForDate(userId, dateFor.ToString("yyyyMMdd"));
        //    var query = new RankUpdatesOnDayQuery(userId, dateFor.ToString("yyyyMMdd"));
        //    var ranksData = (await qRankUpdatesOnDay.Handle(query)).Info;

        //    // Special case with ranks because they need a format to be defined
        //    // When the data is not available, use an empty dictionary instead
        //    if (ranksData.Count == 1 && ranksData.First().Value.rankUpdateType == default)
        //        ranksData = new Dictionary<DateTime, RankUpdatedRaw>();

        //    var result = await Task.WhenAll(ranksData.Select(i => GetDeltaFromRankUpdatedSingle(i.Key, i.Value)));
        //    return result;
        //}

        public async Task<ICollection<EconomyEvent>> LoadEconomyEventsForDate(string userId, DateTime dateFor)
        {
            if (dateFor < dateNewHistory)
                return await GetNewCardsLegacy(userId, dateFor);

            //var (inventoryUpdates, postMatchUpdates) = userHistoryLoaderFromFile.LoadEconomyUpdatesForDate(id, dateString);
            //var inventoryUpdates = (await cacheUserHistoryInventoryUpdated.Get(userId, dateString)).Info;
            //var postMatchUpdates = (await cacheUserHistoryPostMatchUpdates.Get(userId, dateString)).Info;
            var inventoryUpdates = (await qInventoryUpdatesOnDay.Handle(new InventoryUpdatesOnDayQuery(userId, dateFor))).Info;
            var postMatchUpdates = await qPostMatchUpdatesOnDay.Handle(new PostMatchUpdatesOnDayQuery(userId, dateFor));

            var result = inventoryUpdates.Select(i => ConvertInventoryUpdate(i.Key, i.Value))
                .Union(postMatchUpdates.SelectMany(i => ConvertPostMatchUpdate(i.Key, i.Value)))
                //.Where(i => string.IsNullOrWhiteSpace(i.Description) == false)
                .Where(i => i.Changes.Any(x => x.Amount != 0))
                .OrderBy(i => i.DateTime)
                .ToArray();

            return result;
        }

        public async Task<ICollection<EconomyEvent>> LoadEconomyEventsFromDate(string userId, DateTime dateFrom)
        {
            var inventoryUpdates = await qInventoryUpdatesAfter.Handle(new InventoryUpdatesAfterQuery(userId, dateFrom));
            var postMatchUpdates = await qPostMatchUpdatesAfter.Handle(new PostMatchUpdatesAfterQuery(userId, dateFrom));

            var result = inventoryUpdates.Select(i => ConvertInventoryUpdate(i.Item1, i.Item2))
                .Union(postMatchUpdates.SelectMany(i => ConvertPostMatchUpdate(i.Key, i.Value)))
                .Where(i => i.Changes.Any(x => x.Amount != 0))
                .OrderBy(i => i.DateTime)
                .ToArray();

            return result;
        }

        private async Task<ICollection<EconomyEvent>> GetNewCardsLegacy(string userId, DateTime dateFor)
        {
            var newCardsDeprecated = (await qDiff.Handle(new DiffQuery(userId, dateFor))).NewCards;
            if (newCardsDeprecated.Count > 0)
                return new[]
                {
                    new EconomyEvent
                    {
                        DateTime = dateFor,
                        Context = "New cards",
                        NewCards = newCardsDeprecated,
                    }
                };

            return new[]
            {
                new EconomyEvent
                {
                    DateTime = dateFor,
                    Context = "N/A",
                }
            };
        }

        private EconomyEvent ConvertInventoryUpdate(DateTime timestamp, InventoryUpdatedRaw update)
        {
            var result = userHistorySummaryBuilder.BuildFromInventoryUpdates(new[] { update });
            return ConvertFromRaw(update.context, timestamp, result);
        }

        private ICollection<EconomyEvent> ConvertPostMatchUpdate(DateTime timestamp, PostMatchUpdateRaw update)
        {
            var result = userHistorySummaryBuilder.BuildFromPostMatchUpdates(new[] { update });
            return result.Select(i => ConvertFromRaw(string.Join(", ", i.Contexts), timestamp, i)).ToArray();
        }

        private EconomyEvent ConvertPostMatchUpdateMerged(DateTime timestamp, PostMatchUpdateRaw update)
        {
            var result = userHistorySummaryBuilder.BuildFromPostMatchUpdatesMerged(new[] { update });
            return ConvertFromRaw(string.Join(", ", result.Contexts), timestamp, result);
        }
    }
}