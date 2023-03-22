﻿using System;
using System.Collections.Generic;
using MTGAHelper.Entity.Config.Users;
using MTGAHelper.Entity.MtgaOutputLog;
using MTGAHelper.Entity.OutputLogParsing;

namespace MTGAHelper.Entity.UserHistory
{
    public class DateSnapshotInfo
    {
        public DateTime Date { get; set; }

        public HashSet<string> MtgaDecksFound { get; set; } = new HashSet<string>();
        public ICollection<ConfigModelRankInfo> RankSynthetic { get; set; } = Array.Empty<ConfigModelRankInfo>();
        public Inventory Inventory { get; set; } = new Inventory();
        public IReadOnlyDictionary<int, int> Collection { get; set; } = new Dictionary<int, int>();
        public Dictionary<string, PlayerProgress> PlayerProgress { get; set; } = new Dictionary<string, PlayerProgress>();
        public Dictionary<DateTime, GetPlayerProgressRaw> PlayerProgressIntraday { get; set; } = new Dictionary<DateTime, GetPlayerProgressRaw>();
        public ICollection<MatchResult> Matches { get; set; } = Array.Empty<MatchResult>();
        public ICollection<PlayerQuest> PlayerQuests { get; set; } = Array.Empty<PlayerQuest>();
        public ICollection<DraftPickStatusRaw> DraftPickProgress { get; set; } = Array.Empty<DraftPickStatusRaw>();
        public Dictionary<DateTime, DraftPickStatusRaw> DraftPickProgressIntraday { get; set; } = new Dictionary<DateTime, DraftPickStatusRaw>();
        public Dictionary<DateTime, InventoryUpdatedRaw> InventoryUpdates { get; set; } = new Dictionary<DateTime, InventoryUpdatedRaw>();
        public Dictionary<DateTime, PostMatchUpdateRaw> PostMatchUpdates { get; set; } = new Dictionary<DateTime, PostMatchUpdateRaw>();

        //public Dictionary<DateTime, CrackBoosterRaw> CrackedBoosters { get; set; } = new Dictionary<DateTime, CrackBoosterRaw>();
        //public Dictionary<DateTime, CompleteVaultRaw> VaultsOpened { get; set; } = new Dictionary<DateTime, CompleteVaultRaw>();
        public Dictionary<DateTime, EventClaimPrizeRaw> EventClaimPrize { get; set; } = new Dictionary<DateTime, EventClaimPrizeRaw>();

        public Dictionary<DateTime, Inventory> InventoryIntraday { get; set; } = new Dictionary<DateTime, Inventory>();

        //public Dictionary<DateTime, Dictionary<int, int>> CollectionIntraday { get; set; } = new Dictionary<DateTime, Dictionary<int, int>>();
        //public Dictionary<DateTime, RankUpdatedRaw> RankUpdated { get; set; } = new Dictionary<DateTime, RankUpdatedRaw>();

        public Dictionary<DateTime, GetCombinedRankInfoRaw> CombinedRankInfo { get; set; } = new Dictionary<DateTime, GetCombinedRankInfoRaw>();
        //public Dictionary<DateTime, PayEntryRaw> PayEntry { get; set; } = new Dictionary<DateTime, PayEntryRaw>();
        //public Dictionary<DateTime, MythicRatingUpdatedRaw> MythicRatingUpdated { get; set; } = new Dictionary<DateTime, MythicRatingUpdatedRaw>();

        //public int Wins { get; set; }
        //public int Losses { get; set; }
        //public string ConstructedRank { get; set; }
        //public string LimitedRank { get; set; }

        public Dictionary<string, Outcomes> OutcomesByMode { get; set; } = new Dictionary<string, Outcomes>();

        public DateSnapshotInfo(DateTime date)
        {
            Date = date;
        }

        public DateSnapshotInfo()
        {
        }
    }

    //[Serializable]
    //public class ConfigModelUserHistoryEntry
    //{
    //    public Collection Collection { get; set; }
    //    public MtgaUserProfile UserProfile { get; set; } = new MtgaUserProfile();
    //}
}