using MTGAHelper.Entity;
using MTGAHelper.Server.Data.CosmosDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTGAHelper.Tools.CosmosDB.Downloader
{
    public class DateUpdater : ProcessorUserBase
    {
        public DateUpdater(
            UserDataCosmosManager userDataCosmosManager,
            ICollection<string> supporterIds
            )
            : base(userDataCosmosManager, supporterIds)
        {
        }

        public async Task AddDate(string userId, DateTime date)
        {
            var sDate = date.ToString("yyyyMMdd");
            var dates = await DownloadDates(userId);

            if (dates.Contains(sDate))
                return;

            dates = dates
                .Union(new[] { sDate })
                .OrderBy(i => i)
                .ToArray();

            var dataKey = $"{userId}_{InfoByDateKeyEnum.DatesWithData}";
            await userDataCosmosManager.SetDataForUserId(userId, dataKey, dates);
        }
    }
}
