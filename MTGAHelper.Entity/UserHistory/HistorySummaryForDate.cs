﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace MTGAHelper.Entity.UserHistory
{
    public class Outcomes
    {
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int Draws { get; set; }
    }

    public class HistorySummaryForDate
    {
        // From info
        public DateTime Date { get; set; }
        //public ICollection<ConfigModelRankInfo> RankInfo { get; set; } = new ConfigModelRankInfo[0];
        public string ConstructedRank { get; set; }
        public string LimitedRank { get; set; }
        //public int Wins { get; set; }
        //public int Losses { get; set; }
        public Dictionary<string, Outcomes> OutcomesByMode { get; set; }

        // From diff
        public int NewCardsCount { get; set; }
        public int GoldChange { get; set; }
        public int GemsChange { get; set; }
        public float VaultProgressChange { get; set; }
        public Dictionary<RarityEnum, int> WildcardsChange { get; set; } = new Dictionary<RarityEnum, int>
        {
            { RarityEnum.Mythic, 0 },
            { RarityEnum.Rare, 0 },
            { RarityEnum.Uncommon, 0 },
            { RarityEnum.Common, 0 },
        };

        // V2
        public ICollection<string> Contexts { get; set; } = Array.Empty<string>();
        public int XpChange { get; set; }
        public Dictionary<string, int> BoostersChange { get; set; } = new Dictionary<string, int>();
        public Dictionary<int, int> NewCards { get; set; } = new Dictionary<int, int>();

        public RankDelta ConstructedRankChange { get; set; } = new RankDelta();
        public RankDelta LimitedRankChange { get; set; } = new RankDelta();

        public ICollection<EconomyEventChange> DescriptionList => new EconomyEventChange[]
        {
            new EconomyEventChange
            {
                Type = EconomyEventChangeEnum.NewCards,
                Amount = NewCardsCount,
            },
            new EconomyEventChange
            {
                Type = EconomyEventChangeEnum.Gold,
                Amount = GoldChange,
            },
            new EconomyEventChange
            {
                Type = EconomyEventChangeEnum.Gems,
                Amount = GemsChange,
            },
            new EconomyEventChange
            {
                Type = EconomyEventChangeEnum.Vault,
                Amount = VaultProgressChange,
            },
            new EconomyEventChange
            {
                Type = EconomyEventChangeEnum.Xp,
                Amount = XpChange,
            },
        }
            .Union(wildcardsChanges)
            .Union(boosterChanges)
            .Where(i => Convert.ToSingle(i.Amount) != 0f)
            .ToArray();
        public ICollection<string> DescriptionStrings => new[] {
            descNewCardsCount,
            descGold,
            descGems,
            descVault,
            descXp,
        }
            .Union(descWildcards)
            .Union(descBoosters)
            .Where(i => string.IsNullOrWhiteSpace(i) == false)
            .ToArray();

        public string Descriptions => string.Join(", ", DescriptionStrings);

        string descNewCardsCount => NewCardsCount == 0 ? "" : $"{directionStr(NewCardsCount)} new card{(NewCardsCount == 1 ? "" : "s")}";
        string descGold => GoldChange == 0 ? "" : $"{directionStr(GoldChange)} gold";
        string descGems => GemsChange == 0 ? "" : $"{directionStr(GemsChange)} gems";
        string descVault => VaultProgressChange == 0f ? "" : $"{directionStr(VaultProgressChange)}% vault progress";
        string descXp => XpChange == 0 ? "" : $"{directionStr(XpChange)} xp";

        ICollection<string> descWildcards => WildcardsChange?.Where(i => i.Value != 0)?.Select(i => $"{directionStr(i.Value)} {i.Key} wc")?.ToArray() ?? Array.Empty<string>();
        ICollection<EconomyEventChange> wildcardsChanges => WildcardsChange?.Where(i => i.Value != 0)?.Select(i =>
            new EconomyEventChange
            {
                Type = EconomyEventChangeEnum.Wildcard,
                Amount = i.Value,
                Value = i.Key.ToString(),
            })?.ToArray() ?? Array.Empty<EconomyEventChange>();

        ICollection<string> descBoosters => BoostersChange?.Where(i => i.Value != 0)?.Select(i => $"{directionStr(i.Value)} {i.Key} booster")?.ToArray() ?? Array.Empty<string>();
        ICollection<EconomyEventChange> boosterChanges => BoostersChange?.Where(i => i.Value != 0)?.Select(i =>
            new EconomyEventChange
            {
                Type = EconomyEventChangeEnum.Booster,
                Amount = i.Value,
                Value = i.Key.ToString(),
            })?.ToArray() ?? Array.Empty<EconomyEventChange>();

        string directionStr(float f) => f < 0f ? $"Lost {Math.Abs(f)}" : $"Gained {f}";
        string directionStr(double f) => f < 0d ? $"Lost {Math.Abs(f)}" : $"Gained {f}";
        string directionStr(int f) => f < 0 ? $"Lost {Math.Abs(f)}" : $"Gained {f}";
    }
}
