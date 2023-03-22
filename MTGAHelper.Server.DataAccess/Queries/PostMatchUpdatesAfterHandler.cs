using MTGAHelper.Entity.OutputLogParsing;
using MTGAHelper.Server.DataAccess.CacheUserHistory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MTGAHelper.Server.DataAccess.Queries
{
    public class PostMatchUpdatesAfterHandler : IQueryHandler<PostMatchUpdatesAfterQuery, IReadOnlyCollection<KeyValuePair<DateTime, PostMatchUpdateRaw>>>
    {
        private readonly CacheUserHistory<PostMatchUpdateRaw> matchRepo;
        private readonly UserHistoryDatesAvailable datesAvailable;

        public PostMatchUpdatesAfterHandler(CacheUserHistory<PostMatchUpdateRaw> matchRepo, UserHistoryDatesAvailable datesAvailable)
        {
            this.matchRepo = matchRepo;
            this.datesAvailable = datesAvailable;
        }

        public async Task<IReadOnlyCollection<KeyValuePair<DateTime, PostMatchUpdateRaw>>> Handle(PostMatchUpdatesAfterQuery query)
        {
            var fromStr = query.FromDateTime.ToString("yyyyMMdd");
            var datesToLoad = (await datesAvailable.GetDatesRecentFirst(query.UserId))
                .TakeWhile(i => string.Compare(i, fromStr, StringComparison.Ordinal) >= 0);

            var postMatchUpdatesPerDay = await Task.WhenAll(datesToLoad.Select(dateFor => matchRepo.Get(query.UserId, dateFor)));
            return postMatchUpdatesPerDay
                .SelectMany(d => d.Info.OrderByDescending(i => i.Key))
                .Where(kvp => kvp.Key >= query.FromDateTime)
                .ToArray();
        }
    }
}