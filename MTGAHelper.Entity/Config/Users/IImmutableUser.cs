using MTGAHelper.Entity.Config.Decks;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace MTGAHelper.Entity.Config.Users;

public interface IImmutableUser
{
    string PlayerName { get; }

    string Id { get; }

    string LastUploadHash { get; }

    bool HasDeckIdsBeenRecoveredForEachMatch { get; }

    DateTime DateCreatedUtc { get; }

    bool ThemeIsDark { get; }

    string CollectionSetsOrder { get; }

    bool LandsPickAll { get; }

    DateTime LastLoginUtc { get; }

    int NbLogin { get; }

    IReadOnlyDictionary<RarityEnum, UserWeightDto> Weights { get; }

    IReadOnlyDictionary<string, ConfigModelDeck> CustomDecks { get; }

    IReadOnlyDictionary<string, float> PriorityByDeckId { get; }

    IImmutableSet<string> ScrapersActive { get; }

    IReadOnlyList<int> LandsPreference { get; }

    IReadOnlyList<string> NotificationsInactive { get; }

    int NbDailyWinsExpected { get; }

    int NbWeeklyWinsExpected { get; }

    UserDataInMemoryModel DataInMemory { get; }

    bool isDebug { get; }

    bool IsTracked(IDeck deck);
}