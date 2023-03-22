using MTGAHelper.Entity;
using MTGAHelper.Server.DataAccess.CacheUserHistory;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MTGAHelper.Server.DataAccess.Queries
{
    public class LatestPlayerProgressHandler : IQueryHandler<LatestPlayerProgressQuery, InfoByDate<IReadOnlyDictionary<string, PlayerProgress>>>
    {
        private readonly UserHistoryRepositoryGeneric<InfoByDate<Dictionary<string, PlayerProgress>>> cacheUserHistoryPlayerProgress;
        private readonly CacheUserHistoryOld<Dictionary<string, PlayerProgress>> cacheUserHistoryPlayerProgressOld;

        public LatestPlayerProgressHandler(
            UserHistoryRepositoryGeneric<InfoByDate<Dictionary<string, PlayerProgress>>> cacheUserHistoryPlayerProgress,
            CacheUserHistoryOld<Dictionary<string, PlayerProgress>> cacheUserHistoryPlayerProgressOld
            )
        {
            this.cacheUserHistoryPlayerProgress = cacheUserHistoryPlayerProgress.Init(InfoByDateKeyEnum.PlayerProgress.ToString(), false);
            this.cacheUserHistoryPlayerProgressOld = cacheUserHistoryPlayerProgressOld;
        }

        public async Task<InfoByDate<IReadOnlyDictionary<string, PlayerProgress>>> Handle(LatestPlayerProgressQuery query)
        {
            var res = await cacheUserHistoryPlayerProgress.GetData(query.UserId);

            if (res.DateTime == default && res.Info == null)
            {
                // TEMP!!! While transitioning from player progress stored daily
                // to latest player progress stored only
                res = await cacheUserHistoryPlayerProgressOld.GetLast(query.UserId);
            }

            return new InfoByDate<IReadOnlyDictionary<string, PlayerProgress>>(res.DateTime, res.Info);
        }
    }
}