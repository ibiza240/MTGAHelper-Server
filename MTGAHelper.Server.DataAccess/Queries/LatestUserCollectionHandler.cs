using MTGAHelper.Entity;
using MTGAHelper.Server.DataAccess.CacheUserHistory;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MTGAHelper.Server.DataAccess.Queries
{
    public class LatestUserCollectionHandler : IQueryHandler<LatestUserCollectionQuery, InfoByDate<IReadOnlyDictionary<int, int>>>
    {
        private readonly CacheUserHistoryOld<Dictionary<int, int>> cacheUserCollection;
        private readonly UserHistoryRepositoryGeneric<InfoByDate<IReadOnlyDictionary<int, int>>> repositoryCollection;

        public LatestUserCollectionHandler(
            CacheUserHistoryOld<Dictionary<int, int>> cacheUserCollection,
            UserHistoryRepositoryGeneric<InfoByDate<IReadOnlyDictionary<int, int>>> repositoryCollection
            )
        {
            this.cacheUserCollection = cacheUserCollection;
            this.repositoryCollection = repositoryCollection.Init(InfoByDateKeyEnum.Collection.ToString(), false);
        }

        public async Task<InfoByDate<IReadOnlyDictionary<int, int>>> Handle(LatestUserCollectionQuery query)
        {
            var res = await repositoryCollection.GetData(query.UserId);

            if (res.Info == null)
            {
                // TEMP!!! While transitioning from collection stored daily
                // to latest collection stored only
                var res2 = await cacheUserCollection.GetLast(query.UserId);
                res = new InfoByDate<IReadOnlyDictionary<int, int>>
                {
                    DateTime = res2.DateTime,
                    Info = res2.Info,
                };
            }

            return res;
        }
    }
}