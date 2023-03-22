using MTGAHelper.Server.DataAccess.CacheUserHistory;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MTGAHelper.Server.DataAccess.Queries
{
    public class MtgaDecksFoundHandler : IQueryHandler<MtgaDecksFoundQuery, HashSet<string>>
    {
        private readonly CacheUserHistoryOld<HashSet<string>> cache;

        public MtgaDecksFoundHandler(CacheUserHistoryOld<HashSet<string>> cache)
        {
            this.cache = cache;
        }

        public async Task<HashSet<string>> Handle(MtgaDecksFoundQuery query)
        {
            return (await cache.GetLast(query.UserId)).Info;
        }
    }
}