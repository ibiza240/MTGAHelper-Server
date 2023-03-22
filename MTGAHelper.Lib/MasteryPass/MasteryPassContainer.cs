using MTGAHelper.Entity;
using MTGAHelper.Entity.OutputLogParsing;
using MTGAHelper.Server.DataAccess;
using MTGAHelper.Server.DataAccess.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MTGAHelper.Lib.MasteryPass
{
    public class MasteryPassContainer
    {
        private readonly MasteryPassCalculator masteryPassCalculator;
        private readonly IQueryHandler<PostMatchUpdatesAfterQuery, IReadOnlyCollection<KeyValuePair<DateTime, PostMatchUpdateRaw>>> qPostMatchUpdates;
        private readonly IQueryHandler<LatestQuestsQuery, InfoByDate<IReadOnlyList<PlayerQuest>>> qLatestQuests;
        private readonly IQueryHandler<LatestPlayerProgressQuery, InfoByDate<IReadOnlyDictionary<string, PlayerProgress>>> qPlayerProgress;

        public MasteryPassContainer(
            MasteryPassCalculator masteryPassCalculator,
            IQueryHandler<PostMatchUpdatesAfterQuery, IReadOnlyCollection<KeyValuePair<DateTime, PostMatchUpdateRaw>>> qPostMatchUpdates,
            IQueryHandler<LatestQuestsQuery, InfoByDate<IReadOnlyList<PlayerQuest>>> qLatestQuests,
            IQueryHandler<LatestPlayerProgressQuery, InfoByDate<IReadOnlyDictionary<string, PlayerProgress>>> qPlayerProgress)
        {
            this.masteryPassCalculator = masteryPassCalculator;
            this.qPostMatchUpdates = qPostMatchUpdates;
            this.qLatestQuests = qLatestQuests;
            this.qPlayerProgress = qPlayerProgress;
        }

        public async Task<MasteryPassCalculator> Calculate(string userId, string set, int nbDailyWinsExpected, int nbWeeklyWinsExpected)
        {
            var nowUtc = DateTime.UtcNow;
            var progress = await qPlayerProgress.Handle(new LatestPlayerProgressQuery(userId));
            var thisWeeksPostMatchUpdates = await FetchThisWeeksPostMatchUpdates(userId, nowUtc);

            // weekly wins
            var lastWeeklyWinUpdate = thisWeeksPostMatchUpdates
                .Select(kvp => kvp.Value.weeklyWinUpdates)
                .FirstOrDefault(wwu => wwu?.Any() == true);
            var weeklyWinsCompleted = lastWeeklyWinUpdate?.Select(u => int.Parse(u.context.sourceId)).Max() ?? 0;

            // daily wins
            var lastDailyWinUpdate = thisWeeksPostMatchUpdates
                .Where(u => u.Key.Date == nowUtc.Date)
                .Select(u => u.Value.dailyWinUpdates)
                .FirstOrDefault(u => u?.Any() == true);
            var dailyWinsCompleted = lastDailyWinUpdate?.Select(u => int.Parse(u.context.sourceId)).Max() ?? 0;

            // quests
            var quests = await qLatestQuests.Handle(new LatestQuestsQuery(userId));
            //var lastPostMatchWithQuests = thisWeeksPostMatchUpdates.FirstOrDefault();
            //var (openQuests, onDate) = lastPostMatchWithQuests.Value != null && lastPostMatchWithQuests.Key > quests.DateTime
            //    ? (lastPostMatchWithQuests.Value.questUpdate.Count(quest => quest.endingProgress < quest.goal), lastPostMatchWithQuests.Key)
            //    : (quests.Info.Count, quests.DateTime);
            //var nbOpenQuests = Math.Min(MasteryPassCalculator.NB_QUESTS, openQuests + (int)(nowUtc.Date - onDate.Date).TotalDays);
            var nbOpenQuests = quests.Info.Count;

            // current XP and level
            var progressCurrent = progress.Info.Count == 1 ?
                progress.Info.Single().Value :
                progress.Info.FirstOrDefault(i => i.Key.Contains("BattlePass", StringComparison.InvariantCultureIgnoreCase)).Value ?? new PlayerProgress();

            var currentLevel = progressCurrent.CurrentLevel + 1;
            var currentXp = progressCurrent.CurrentExp;

            var lastPostMatchWithMasteryPassInfo = thisWeeksPostMatchUpdates
                .FirstOrDefault(u => u.Value.battlePassUpdate != null);
            if (lastPostMatchWithMasteryPassInfo.Key > progress.DateTime
                && (lastPostMatchWithMasteryPassInfo.Value?.battlePassUpdate?.trackDiff?.currentLevel ?? 0) != 0)
            {
                // Level is now 0 based index
                currentLevel = lastPostMatchWithMasteryPassInfo.Value.battlePassUpdate.trackDiff.currentLevel + 1;
                currentXp = lastPostMatchWithMasteryPassInfo.Value.battlePassUpdate.trackDiff.currentExp;
            }

            var inputs = new MasteryPassCalculatorInputs
            {
                CurrentDateUtc = nowUtc,
                CurrentLevel = currentLevel,
                CurrentXp = currentXp,
                DailyQuestsAvailable = nbOpenQuests,
                DailyWinsCompleted = dailyWinsCompleted,
                ExpectedDailyWins = nbDailyWinsExpected,
                ExpectedWeeklyWins = nbWeeklyWinsExpected,
                WeeklyWinsCompleted = weeklyWinsCompleted,
            };

            masteryPassCalculator.EstimateFinalLevel(set, inputs);

            return masteryPassCalculator;
        }

        private async Task<IReadOnlyCollection<KeyValuePair<DateTime, PostMatchUpdateRaw>>> FetchThisWeeksPostMatchUpdates(string userId, DateTime nowUtc)
        {
            var lastWeeklyWinResetUtc = MasteryPassCalculator.CalculateLastWeeklyWinResetUtc(nowUtc);
            return await qPostMatchUpdates.Handle(new PostMatchUpdatesAfterQuery(userId, lastWeeklyWinResetUtc));
        }
    }
}