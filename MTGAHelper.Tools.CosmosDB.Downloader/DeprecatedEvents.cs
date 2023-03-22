using System;
using System.Collections.Generic;
using System.Text;

namespace MTGAHelper.Tools.CosmosDB.Downloader
{
    internal class RankUpdatedRaw
    {
        public string playerId { get; set; }
        public int seasonOrdinal { get; set; }
        public string newClass { get; set; }
        public string oldClass { get; set; }
        public int newLevel { get; set; }
        public int oldLevel { get; set; }
        public int oldStep { get; set; }
        public int newStep { get; set; }
        public bool wasLossProtected { get; set; }
        public string rankUpdateType { get; set; }
    }

    internal class MythicRatingUpdatedRaw
    {
        public float oldMythicPercentile { get; set; }
        public float newMythicPercentile { get; set; }
        public int oldMythicLeaderboardPlacement { get; set; }
        public int newMythicLeaderboardPlacement { get; set; }
        public string context { get; set; }
    }
}
