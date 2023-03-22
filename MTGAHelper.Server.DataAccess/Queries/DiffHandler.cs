using MTGAHelper.Entity.UserHistory;
using MTGAHelper.Server.DataAccess.CacheUserHistory;
using System.Threading.Tasks;

namespace MTGAHelper.Server.DataAccess.Queries
{
    public class DiffHandler : IQueryHandler<DiffQuery, DateSnapshotDiff>
    {
        private readonly CacheUserHistoryOld<DateSnapshotDiff> cacheUserHistoryDiff;

        public DiffHandler(CacheUserHistoryOld<DateSnapshotDiff> cacheUserHistoryDiff)
        {
            this.cacheUserHistoryDiff = cacheUserHistoryDiff;
        }

        public async Task<DateSnapshotDiff> Handle(DiffQuery query)
        {
            var infoByDate = await cacheUserHistoryDiff.Get(query.UserId, query.Date.ToString("yyyyMMdd"));
            return infoByDate.Info;
        }
    }
}