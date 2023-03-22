using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MTGAHelper.Entity;
using MTGAHelper.Server.Data;
using MTGAHelper.Server.Data.CosmosDB;
using MTGAHelper.Server.Data.Files.UserHistory;

namespace MTGAHelper.Server.DataAccess
{
    public class StatsLimitedRepository
    {
        private readonly UserDataCosmosManager userDataCosmosManager;

        public StatsLimitedRepository(
            UserDataCosmosManager userDataCosmosManager
        )
        {
            this.userDataCosmosManager = userDataCosmosManager;
        }

        public async Task<CosmosDataStatsLimited> GetStatsLimited(string userId)
        {
            var dataKey = $"{userId}_{InfoByDateKeyEnum.StatsLimited}";
            var response = await userDataCosmosManager.GetDataForUserId<CosmosDataStatsLimited>(userId, dataKey);
            return response.data;
        }

        public async Task SetStatsLimited(string userId, CosmosDataStatsLimited stats)
        {
            var dataKey = $"{userId}_{InfoByDateKeyEnum.StatsLimited}";
            await userDataCosmosManager.SetDataForUserId(userId, dataKey, stats);
        }

        public async Task UpdateIsUpToDate(string userId, bool isUpToDate)
        {
            var dataKey = $"{userId}_{InfoByDateKeyEnum.StatsLimited}";

            try
            {
                var existing = await userDataCosmosManager.GetDataForUserId<CosmosDataStatsLimited>(userId, dataKey);
                if (existing.found)
                {
                    existing.data.IsUpToDate = isUpToDate;
                    await userDataCosmosManager.SetDataForUserId(userId, dataKey, existing.data);
                }
            }
            catch (Exception ex)
            {
                await userDataCosmosManager.SetDataForUserId(userId, dataKey, new CosmosDataStatsLimited());
            }
        }
    }
}