using MTGAHelper.Entity.MtgaOutputLog;
using System.Threading.Tasks;

namespace MTGAHelper.Lib
{
    public partial class UserManager
    {
        //Dictionary<Type, InfoByDateKeyEnum> DictTypeToEnum = new Dictionary<Type, InfoByDateKeyEnum>
        //{
        //    { typeof(List<ConfigModelRawDeck>), InfoByDateKeyEnum.Decks },

        //    { typeof(InfoByDate<Dictionary<DateTime, Dictionary<int, int>>>), InfoByDateKeyEnum.CollectionIntraday },
        //    { typeof(InfoByDate<Dictionary<DateTime, GetCombinedRankInfoRaw>>), InfoByDateKeyEnum.CombinedRankInfo },
        //    { typeof(InfoByDate<Dictionary<DateTime, CrackBoosterRaw>>), InfoByDateKeyEnum.CrackedBoosters },
        //    { typeof(InfoByDate<Dictionary<DateTime, DraftMakePickRaw>>), InfoByDateKeyEnum.DraftPickProgressIntraday },
        //    { typeof(InfoByDate<Dictionary<DateTime, EventClaimPrizeRaw>>), InfoByDateKeyEnum.EventClaimPrice },
        //    { typeof(InfoByDate<Dictionary<DateTime, Inventory>>), InfoByDateKeyEnum.InventoryIntraday },
        //    { typeof(InfoByDate<Dictionary<DateTime, InventoryUpdatedRaw>>), InfoByDateKeyEnum.InventoryUpdates },
        //    { typeof(InfoByDate<Dictionary<DateTime, MythicRatingUpdatedRaw>>), InfoByDateKeyEnum.MythicRatingUpdated },
        //    { typeof(InfoByDate<Dictionary<DateTime, PayEntryRaw>>), InfoByDateKeyEnum.PayEntry },
        //    { typeof(InfoByDate<Dictionary<DateTime, GetPlayerProgressRaw>>), InfoByDateKeyEnum.PlayerProgressIntraday },
        //    { typeof(InfoByDate<Dictionary<DateTime, PostMatchUpdateRaw>>), InfoByDateKeyEnum.PostMatchUpdates },
        //    { typeof(InfoByDate<Dictionary<DateTime, RankUpdatedRaw>>), InfoByDateKeyEnum.RankUpdated },
        //    { typeof(InfoByDate<Dictionary<DateTime, CompleteVaultRaw>>), InfoByDateKeyEnum.VaultsOpened },

        //    { typeof(InfoByDate<HashSet<string>>), InfoByDateKeyEnum.MtgaDecksFound },
        //    { typeof(InfoByDate<Dictionary<int, int>>), InfoByDateKeyEnum.Collection },
        //    { typeof(InfoByDate<Inventory>), InfoByDateKeyEnum.Inventory },
        //    { typeof(InfoByDate<DateSnapshotDiff>), InfoByDateKeyEnum.Diff },
        //    { typeof(InfoByDate<List<ConfigModelRankInfo>>), InfoByDateKeyEnum.Rank },
        //    { typeof(InfoByDate<List<MatchResult>>), InfoByDateKeyEnum.Matches },
        //    { typeof(InfoByDate<Dictionary<string, PlayerProgress>>), InfoByDateKeyEnum.PlayerProgress },
        //    { typeof(InfoByDate<List<PlayerQuest>>), InfoByDateKeyEnum.PlayerQuests },
        //    //{ typeof(InfoByDate<List<DraftMakePickRaw>>), InfoByDateKeyEnum.DraftPickProgress },
        //};

        public async Task SaveNewInfo(string userId, OutputLogResult newOutputLogResult)
        {
            var configUser = await ConfigUsers.MutateUser(userId)
                .UpdateFromOutputLogResult(newOutputLogResult.PlayerName, newOutputLogResult.LastUploadHash);

            await logResultPersister.SaveHistoryToDisk(configUser, newOutputLogResult);
        }
    }
}