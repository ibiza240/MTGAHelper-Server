using MTGAHelper.Entity.CollectionDecksCompare;
using MTGAHelper.Entity.Config.Decks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace MTGAHelper.Entity.Config.Users
{
    public enum FormatEnum
    {
        Unknown,
        Standard,
        Historic
    }

    public enum UserPreferenceEnum
    {
        Unknown,
        ThemeIsDark,
        CollectionSetsOrder,
        LandsPickAll,
    }

    [Serializable]
    public sealed record ConfigModelUser : IImmutableUser

    {
    public const string USER_LOCAL = "localuser";
    public const string DEFAULT_SCRAPER = "mtggoldfish-meta-standard";

    public string PlayerName { get; init; } = "";

    public string Id { get; init; } = USER_LOCAL;

    public string LastUploadHash { get; init; } = "";

    public bool HasDeckIdsBeenRecoveredForEachMatch { get; init; }

    public DateTime DateCreatedUtc { get; init; } = DateTime.UtcNow;

    // Preferences, to refactor
    public bool ThemeIsDark { get; init; } = true;

    public string CollectionSetsOrder { get; init; } = "NewestFirst";

    public bool LandsPickAll { get; init; } = false;
    //public FormatEnum Format { get; init; } = FormatEnum.Standard;

    public DateTime LastLoginUtc { get; init; } = DateTime.UtcNow;

    public int NbLogin { get; init; }

    public IReadOnlyDictionary<RarityEnum, UserWeightDto> Weights { get; init; } = CardRequiredInfo.DEFAULT_WEIGHTS;
    
    //public MtgaUserProfile MtgaUserProfile { get; init; } = new MtgaUserProfile();

    public ImmutableDictionary<string, ConfigModelDeck> CustomDecks { get; init; } = ImmutableDictionary.Create<string, ConfigModelDeck>();

    IReadOnlyDictionary<string, ConfigModelDeck> IImmutableUser.CustomDecks => this.CustomDecks;

    public IReadOnlyDictionary<string, float> PriorityByDeckId { get; init; } = new Dictionary<string, float>();

    public IImmutableSet<string> ScrapersActive { get; init; } = ImmutableSortedSet.Create<string>();

    public IReadOnlyList<int> LandsPreference { get; init; } = new List<int>();

    public IReadOnlyList<string> NotificationsInactive { get; init; } = new List<string>();

    public int NbDailyWinsExpected { get; init; } = 10;

    public int NbWeeklyWinsExpected { get; init; } = 15;

    //public ICollection<ConfigModelRawDeck> MtgaDecks { get; init; }

    //public ICollection<ConfigModelRankInfo> Ranks { get; init; }

    [JsonIgnore] public UserDataInMemoryModel DataInMemory { get; } = new();

    [JsonIgnore] public bool isDebug { get; init; } = false;

    public ConfigModelUser()
    {
        // For serialization
    }

    public ConfigModelUser(string userId)
    {
        // When creating a new user
        Id = userId;
        ScrapersActive = ScrapersActive.Add(DEFAULT_SCRAPER);
    }

    public bool IsTracked(IDeck deck)
    {
        return (deck.ScraperType.Type == ScraperTypeEnum.UserCustomDeck || ScrapersActive.Contains(deck.ScraperType.Id))
            && (PriorityByDeckId.ContainsKey(deck.Id) == false || PriorityByDeckId[deck.Id] > 0);
    }
    }
}