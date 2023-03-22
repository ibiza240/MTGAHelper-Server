using MTGAHelper.Server.Data.CosmosDB;
using MTGAHelper.Tools.CosmosDB.Downloader.v2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MTGAHelper.Tools.CosmosDB.Downloader
{
    public class Orchestrator : BaseDownloader
    {
        private readonly UserDataCosmosManager userDataCosmosManager;
        private ICollection<string> supportersUserIds;

        public Orchestrator(
            UserDataCosmosManager userDataCosmosManager
            )
            : base(userDataCosmosManager)
        {
        }

        public async Task DownloadAllData(ICollection<string> supportersUserIds, string folderInputUserIds, string folderOutputCosmosData)
        {
            this.supportersUserIds = supportersUserIds;
            var userIds = GetUserIds(folderInputUserIds);
            //.Where(i => i == "40790d46e87c4ac5910b7b4e997a52cf").ToArray();

            //await DownloadGroup(0, userIds, folderOutputCosmosData);
            var maxAtSameTime = 10;
            var groupSize = userIds.Count / maxAtSameTime + 1;
            for (int i = 0; i < maxAtSameTime; i++)
            {
                await Task.Factory.StartNew(async () => await DownloadGroup(i, userIds.Skip(groupSize * i).Take(groupSize), folderOutputCosmosData));
            }
        }

        internal async Task CleanOldData(ICollection<string> supportersUserIds, string folderInputUserIds, string folderOutputCosmosData)
        {
            this.supportersUserIds = supportersUserIds;
            var userIds = GetUserIds(folderInputUserIds);

            await CleanGroup(userIds, folderOutputCosmosData);
            //var maxAtSameTime = 8;
            //var groupSize = userIds.Count / maxAtSameTime + 1;
            //for (int i = 0; i < maxAtSameTime; i++)
            //{
            //    await Task.Factory.StartNew(async () => await DownloadGroup(i, userIds.Skip(groupSize * i).Take(groupSize), folderOutputCosmosData));
            //}
        }

        internal async Task AddDateToUser(string userId, DateTime date)
        {
            await new DateUpdater(userDataCosmosManager, null).AddDate(userId, date);
        }

        private async Task DownloadGroup(int iThread, IEnumerable<string> userIds, string folderOutputCosmosData)
        {
            var cnts = new[]
            {
                1,
                1001,
                2001,
                3001,
                4001,
                5001,
                6001,
                7001,
                8001,
                9001,
                10001,
            };

            int iCnt = cnts[iThread];

            var total = userIds.Count();
            foreach (var userId in userIds.Skip(iCnt - 1)/*.Take(50)*/)
            {
                Console.WriteLine($"[{iThread}] {iCnt} / {total}");
                await new ProcessorUser(folderOutputCosmosData, userDataCosmosManager, supportersUserIds/*, true*/).ProcessUser(userId);
                iCnt++;
            }

            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine($"[{iThread}] DONE");
            Console.WriteLine("");
            Console.WriteLine("");
        }

        private async Task CleanGroup(IEnumerable<string> userIds, string folderOutputCosmosData)
        {
            int iCnt = 1;
            var total = userIds.Count();
            foreach (var userId in userIds.Skip(iCnt - 1)/*.Take(50)*/)
            {
                Console.WriteLine($"{iCnt} / {total}");
                await new CosmosCleaner(userDataCosmosManager, supportersUserIds, folderOutputCosmosData).ProcessUser(userId/*, true*/);
                iCnt++;
            }
        }
    }
}