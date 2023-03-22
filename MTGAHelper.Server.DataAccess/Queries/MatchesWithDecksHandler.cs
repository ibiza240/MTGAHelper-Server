using MTGAHelper.Entity.MtgaOutputLog;
using MTGAHelper.Server.DataAccess.CacheUserHistory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MTGAHelper.Server.DataAccess.Queries
{
    public class MatchesWithDecksHandler : IQueryHandler<MatchesWithDecksQuery, IReadOnlyList<MatchResult>>
    {
        private readonly CacheUserHistoryOld<List<MatchResult>> cacheUserHistoryMatches;

        public MatchesWithDecksHandler(
            CacheUserHistoryOld<List<MatchResult>> cacheUserHistoryMatches)
        {
            this.cacheUserHistoryMatches = cacheUserHistoryMatches;
        }

        public async Task<IReadOnlyList<MatchResult>> Handle(MatchesWithDecksQuery query)
        {
            var decks = new HashSet<string>();
            if (query.Decks != null)
                decks = new HashSet<string>(query.Decks);

            //var data = (await cacheUserHistoryMatches.GetAll(query.UserId))
            //    .SelectMany(i => i.Info);
            var date = query.DateStart.Date;
            var dateMax = DateTime.Now.Date.AddDays(1);
            var data = new List<MatchResult>();
            while (date < dateMax)
            {
                var dataForDate = await cacheUserHistoryMatches.Get(query.UserId, date.ToString("yyyyMMdd"));
                data.AddRange(dataForDate.Info);
                date = date.AddDays(1);
            }

            var dataFiltered = data
                //matchesCacheManager.GetMatches(userId)
                .Where(i => query.Decks == null || (i.DeckUsed != null && decks.Contains(i.DeckUsed.Id)))
                .ToList();

            return dataFiltered;
        }
    }
}