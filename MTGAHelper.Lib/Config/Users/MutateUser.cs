using MTGAHelper.Entity;
using MTGAHelper.Entity.Config.Decks;
using MTGAHelper.Entity.Config.Users;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace MTGAHelper.Lib.Config.Users;

public interface IMutateUser
{
    Task CountLogin(DateTime utcNow);
    Task<IImmutableUser> SetNbExpectedWins(int dailyWins, int weeklyWins);
    Task<IImmutableUser> UpdateFromOutputLogResult(string playerName, uint lastUploadHash);
    Task UpdateLandPreference(IEnumerable<int> lands);
    Task SetDeckPriorityFactor(string deckId, float value);
    Task SetDeckPriorityFactors(IEnumerable<KeyValuePair<string,float>> changes);
    Task AddCustomDeck(string deckId, ConfigModelDeck configModelDeck);
    Task TryRemoveCustomDeck(string deckId);
    Task SetPreferenceThemeIsDark(bool isDark);
    Task SetPreferenceCollectionSetsOrder(string value);
    Task SetPreferenceLandsPickAll(bool doPickAll);
    Task PutScrapersActive(IEnumerable<string> scrapersActive);
    Task AddScraperActive(string activate);
    Task RemoveScraperActive(string remove);
    Task PutUserWeights(IReadOnlyDictionary<RarityEnum,UserWeightDto> weights);
    Task StopNotification(string notificationId);
    Task ResetNotifications();
}

public sealed class MutateUser : IMutateUser
{
    private readonly string userId;
    private readonly ConfigManagerUsers manager;

    internal MutateUser(string userId, ConfigManagerUsers manager)
    {
        this.userId = userId;
        this.manager = manager;
    }

    private Task<IImmutableUser> UpdateUser(Func<ConfigModelUser, ConfigModelUser> update) =>
        manager.UpdateUser(userId, update);

    public Task CountLogin(DateTime utcNow)
    {
        return UpdateUser(user => user with {NbLogin = user.NbLogin + 1, LastLoginUtc = utcNow});
    }

    public Task<IImmutableUser> SetNbExpectedWins(int dailyWins, int weeklyWins)
    {
        if (dailyWins < 0 && weeklyWins < 0)
        {
            return Task.FromResult(manager.Get(userId));
        }

        return UpdateUser(
                user =>
                {
                    // Save new values if initialized and different from config
                    dailyWins = dailyWins >= 0 ? dailyWins : user.NbDailyWinsExpected;
                    weeklyWins = weeklyWins >= 0 ? weeklyWins : user.NbWeeklyWinsExpected;

                    return user with {NbDailyWinsExpected = dailyWins, NbWeeklyWinsExpected = weeklyWins};
                })
            ;
    }

    public Task<IImmutableUser> UpdateFromOutputLogResult(string playerName, uint lastUploadHash)
    {
        return UpdateUser(
            user =>
            {
                var updated = user with {LastUploadHash = lastUploadHash.ToString()};

                if (string.IsNullOrWhiteSpace(playerName) == false)
                {
                    if (string.IsNullOrWhiteSpace(user.PlayerName))
                        updated = updated with {PlayerName = playerName};
                    else if (user.PlayerName != playerName)
                        Log.Warning(
                            "User {userId} ({playerNameCurrent}) tried to upload data for a new player name {playerNameNew}",
                            user.Id,
                            user.PlayerName,
                            playerName);
                }
                return updated;
            });
    }

    public Task UpdateLandPreference(IEnumerable<int> lands)
    {
        return UpdateUser(user => user with {LandsPreference = lands.ToArray()});
    }

    public Task SetDeckPriorityFactor(string deckId, float value)
    {
        return UpdateUser(
            user =>
            {
                ImmutableDictionary<string, float> newDict =
                    user.PriorityByDeckId.ToImmutableDictionary().SetItem(deckId, value);
                return user with {PriorityByDeckId = newDict};
            });
    }

    public Task SetDeckPriorityFactors(IEnumerable<KeyValuePair<string,float>> changes)
    {
        return UpdateUser(
            user =>
            {
                ImmutableDictionary<string, float> newDict =
                    user.PriorityByDeckId.ToImmutableDictionary().SetItems(changes);
                return user with {PriorityByDeckId = newDict};
            });
    }

    public Task AddCustomDeck(string deckId, ConfigModelDeck configModelDeck)
    {
        return UpdateUser(user => user with {CustomDecks = user.CustomDecks.Add(deckId, configModelDeck)});
    }

    public Task TryRemoveCustomDeck(string deckId)
    {
        return UpdateUser(user => user with {CustomDecks = user.CustomDecks.Remove(deckId)});
    }

    public Task SetPreferenceThemeIsDark(bool isDark)
    {
        return UpdateUser(user => user with {ThemeIsDark = isDark});
    }

    public Task SetPreferenceCollectionSetsOrder(string value)
    {
        return UpdateUser(user => user with {CollectionSetsOrder = value});
    }

    public Task SetPreferenceLandsPickAll(bool doPickAll)
    {
        return UpdateUser(user => user with {LandsPickAll = doPickAll});
    }

    public Task PutScrapersActive(IEnumerable<string> scrapersActive)
    {
        return UpdateUser(user => user with {ScrapersActive = scrapersActive.ToImmutableSortedSet()});
    }

    public Task AddScraperActive(string activate)
    {
        return UpdateUser(user => user with {ScrapersActive = user.ScrapersActive.Add(activate)});
    }
    
    public Task RemoveScraperActive(string remove)
    {
        return UpdateUser(user => user with {ScrapersActive = user.ScrapersActive.Remove(remove)});
    }

    public Task PutUserWeights(IReadOnlyDictionary<RarityEnum, UserWeightDto> weights)
    {
        return UpdateUser(u => u with {Weights = weights});
    }

    public Task StopNotification(string notificationId)
    {
        return UpdateUser(
            u => u with {NotificationsInactive = u.NotificationsInactive.Append(notificationId).ToArray()});
    }

    public Task ResetNotifications()
    {
        return UpdateUser( u => u with {NotificationsInactive = Array.Empty<string>()});
    }
}