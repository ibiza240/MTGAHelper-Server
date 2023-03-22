using MTGAHelper.Entity.OutputLogParsing;
using MTGAHelper.Server.DataAccess.CacheUserHistory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MTGAHelper.Server.DataAccess.Queries
{
    public class CompletedDraftsAfterHandler : IQueryHandler<CompletedDraftsAfterQuery, IEnumerable<(DateTime, DraftPickStatusRaw)>>
    {
        private readonly CacheUserHistory<DraftPickStatusRaw> cacheUserHistoryDraftPickProgressIntraday;

        public CompletedDraftsAfterHandler(CacheUserHistory<DraftPickStatusRaw> cacheUserHistoryDraftPickProgressIntraday)
        {
            this.cacheUserHistoryDraftPickProgressIntraday = cacheUserHistoryDraftPickProgressIntraday;
        }

        public async Task<IEnumerable<(DateTime, DraftPickStatusRaw)>> Handle(CompletedDraftsAfterQuery query)
        {
            var dateMax = DateTime.UtcNow.AddDays(1).Date;
            var datesToFetch = GetDateRange(query.FromDateTime, dateMax);
            var tasks = datesToFetch.Select(d =>
                cacheUserHistoryDraftPickProgressIntraday.Get(query.UserId, d.ToString("yyyyMMdd")));
            var draftsInPeriod = await Task.WhenAll(tasks);
            var draftsCompleted = draftsInPeriod
                .SelectMany(d => d.Info)
                .Where(i => i.Value?.DraftStatus?.Contains("Complete") == true)
                .Select(kvp => (kvp.Key, kvp.Value))
                .ToArray();

            return draftsCompleted;
        }

        private static IEnumerable<DateTime> GetDateRange(DateTime firstDay, DateTime lastDayInclusive)
        {
            while (firstDay <= lastDayInclusive)
            {
                yield return firstDay;
                firstDay = firstDay.AddDays(1);
            }
        }
    }
}