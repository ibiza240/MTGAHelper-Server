using MTGAHelper.Entity;
using MTGAHelper.Entity.OutputLogParsing;
using MTGAHelper.Server.DataAccess;
using MTGAHelper.Server.DataAccess.Queries;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MTGAHelper.Lib.UserHistory
{
    public class UserCollectionFetcher
    {
        private readonly IQueryHandler<LatestUserCollectionQuery, InfoByDate<IReadOnlyDictionary<int, int>>> qUserCollection;
        private readonly IQueryHandler<InventoryUpdatesAfterQuery, IEnumerable<(DateTime, InventoryUpdatedRaw)>> qInventoryUpdatesAfter;

        public UserCollectionFetcher(
            IQueryHandler<LatestUserCollectionQuery, InfoByDate<IReadOnlyDictionary<int, int>>> qUserCollection,
            IQueryHandler<InventoryUpdatesAfterQuery, IEnumerable<(DateTime, InventoryUpdatedRaw)>> qInventoryUpdatesAfter)
        {
            this.qUserCollection = qUserCollection;
            this.qInventoryUpdatesAfter = qInventoryUpdatesAfter;
        }

        public async Task<InfoByDate<IReadOnlyDictionary<int, int>>> GetLatestCollection(string userId)
        {
            Log.Debug("Loading collection for {userId} from memory", userId);
            var collection = await StopwatchOperation("latestCollection", () => qUserCollection.Handle(new LatestUserCollectionQuery(userId)));
            //var inventoryUpdatesAfter = await StopwatchOperation("inventoryUpdates", () => qInventoryUpdatesAfter.Handle(new QueryInventoryUpdatesAfter(userId, colDate)));

            return collection;
        }

        private async Task<T> StopwatchOperation<T>(string name, Func<Task<T>> action)
        {
            var sw = Stopwatch.StartNew();

            var ret = await action();

            sw.Stop();
            if (sw.ElapsedMilliseconds >= 1000)
                Log.Information("UserCollectionFetcher Stopwatch {name} {time} s", name, (sw.ElapsedMilliseconds / 1000d).ToString("0.00"));

            return ret;
        }
    }
}