using MTGAHelper.Entity;
using MTGAHelper.Server.DataAccess.CacheUserHistory;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MTGAHelper.Server.DataAccess.Queries
{
    public class LatestQuestsHandler : IQueryHandler<LatestQuestsQuery, InfoByDate<IReadOnlyList<PlayerQuest>>>
    {
        private readonly CacheUserHistoryOld<List<PlayerQuest>> cacheUserHistoryQuests;

        public LatestQuestsHandler(CacheUserHistoryOld<List<PlayerQuest>> cacheUserHistoryQuests)
        {
            this.cacheUserHistoryQuests = cacheUserHistoryQuests;
        }

        public async Task<InfoByDate<IReadOnlyList<PlayerQuest>>> Handle(LatestQuestsQuery query)
        {
            var infoByDate = await cacheUserHistoryQuests.GetLast(query.UserId);
            return new InfoByDate<IReadOnlyList<PlayerQuest>>(infoByDate.DateTime, infoByDate.Info.AsReadOnly());
        }
    }
}