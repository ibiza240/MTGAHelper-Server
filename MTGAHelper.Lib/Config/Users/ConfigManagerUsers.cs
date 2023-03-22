using Microsoft.Extensions.Options;
using MTGAHelper.Entity.Config.App;
using MTGAHelper.Entity.Config.Users;
using MTGAHelper.Server.Data.Files;
using Newtonsoft.Json;
using Nito.AsyncEx;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MTGAHelper.Lib.Config.Users
{
    public interface IConfigManagerUsers
    {
        IEnumerable<IImmutableUser> Values { get; }

        IImmutableUser Get(string id);

        void Set(ConfigModelUser config);

        void Remove(string id);

        void FreeMemory();

        IMutateUser MutateUser(string id);
        Task<IImmutableUser> LoadUserIfNotSet(string id, Func<Task<IImmutableUser>> loadUser);
    }

    public class ConfigManagerUsers : IConfigManagerUsers
    {
        private class UserWithLock
        {
            internal readonly AsyncLock mutateLock;
            internal ConfigModelUser user;

            public UserWithLock(ConfigModelUser user)
            {
                this.mutateLock = new();
                this.user = user;
            }
        }

        private readonly object lockUsers = new();

        private readonly IConfigUsersPath configPath;
        private readonly IOptionsMonitor<ConfigModelApp> appConfig;

        private readonly ConcurrentDictionary<string, Task<IImmutableUser>> loadingUsers = new();
        private Dictionary<string, UserWithLock> cacheData = new();

        public IEnumerable<IImmutableUser> Values
        {
            get
            {
                lock (lockUsers) return cacheData.Values.Select(x => x.user);
            }
        }

        public ConfigManagerUsers(IConfigUsersPath configPath, IOptionsMonitor<ConfigModelApp> appConfig)
        {
            this.configPath = configPath;
            this.appConfig = appConfig;
        }

        public IImmutableUser Get(string id)
        {
            if (id == null) return null;

            lock (lockUsers)
            {
                if (cacheData.ContainsKey(id) == false)
                    return null;

                return cacheData[id].user;
            }
        }

        public async Task<IImmutableUser> LoadUserIfNotSet(string id, Func<Task<IImmutableUser>> loadUser)
        {
            lock (lockUsers)
            {
                if (cacheData.TryGetValue(id, out UserWithLock user) && user is not null)
                {
                    var lastLogin =
                        $"{(DateTime.UtcNow - user.user.LastLoginUtc).TotalHours.ToString("#,0.0")} hours ago";
                    Log.Information(
                        "Config for user {userId} found in memory - Last login was {lastLogin}",
                        user.user.Id,
                        lastLogin);
                    return user.user;
                }
            }

            if (loadingUsers.TryGetValue(id, out Task<IImmutableUser> userLoading))
            {
                Log.Information("User {userId} already loading; waiting for loading to complete", id);
                return await userLoading;
            }
            Task<IImmutableUser> loading = loadingUsers.GetOrAdd(id, _ => loadUser());
            return await loading;
        }

        public IMutateUser MutateUser(string id)
        {
            return new MutateUser(id, this);
        }

        internal async Task<IImmutableUser> UpdateUser(string id, Func<ConfigModelUser, ConfigModelUser> update)
        {
            UserWithLock user;
            lock (lockUsers)
            {
                user = cacheData[id];
            }

            using (await user.mutateLock.LockAsync())
            {
                var updated = update(user.user);
                user.user = updated;
                await SaveToDisk(updated);
                return updated;
            }
        }

        public void Set(ConfigModelUser config)
        {
            UserWithLock user;
            lock (lockUsers)
            {
                if (!cacheData.ContainsKey(config.Id))
                {
                    // easy and expected path
                    cacheData[config.Id] = new UserWithLock(config);
                    return;
                }
                // user is already in the cache...
                // in this case we do NOT want to insert a new UserWithLock
                // as that would introduce more than one lock per user!
                user = cacheData[config.Id];
                Log.Warning(
                    "About to set user {UserId} with existing data in memory! Old user data {UserData}",
                    config.Id,
                    user.user);
            }
            using (user.mutateLock.Lock())
            {
                Log.Warning(
                    "Setting user {UserId} with existing data in memory! New user data {UserData}",
                    config.Id,
                    config);
                user.user = config;
            }
        }

        public void Remove(string id)
        {
            lock (lockUsers)
            {
                if (cacheData.ContainsKey(id))
                    cacheData.Remove(id);
                loadingUsers.TryRemove(id, out _);
            }
        }

        private async Task SaveToDisk(ConfigModelUser userConfig)
        {
            // Clean to remove entries with 1
            var prioritiesCleaned = userConfig.PriorityByDeckId
                .Where(i => i.Value != 1.0f)
                .ToDictionary(i => i.Key, i => i.Value);
            // Save space by removing weights if they are default values
            var weightsSparse = userConfig.Weights.IsDefaultValues() ? null : userConfig.Weights;
            userConfig = userConfig with { PriorityByDeckId = prioritiesCleaned, Weights = weightsSparse };

            //sw.Stop();
            //Log.Information("SW6 {sw1} s", (sw.ElapsedMilliseconds / 1000d).ToString("0.00"));

            var folderUser = Path.Combine(configPath.FolderDataConfigUsers, userConfig.Id);
            Directory.CreateDirectory(folderUser);

            var fileLoader = new FileLoader();
            var userConfigJson = JsonConvert.SerializeObject(userConfig);
            var filePath = Path.Combine(folderUser, $"{userConfig.Id}_userconfig.json");
            // The loop is to try to fix empty files being saved
            var mustSaveFile = true;
            int iTry = 0;
            while (mustSaveFile)
            {
                try
                {
                    await fileLoader.SaveToDiskAsync(filePath, userConfigJson, userConfig.Id, false);

                    mustSaveFile = false;

                    if (appConfig.CurrentValue.SpecialDebugLogUsers.Contains(userConfig.Id))
                    {
                        Log.Information("Saved user {UserId} data {UserData}", userConfig.Id, userConfigJson);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error while saving user config {userId} to disk", userConfig.Id);
                    iTry++;
                    mustSaveFile = iTry < 5;
                    await Task.Delay(iTry * 50);
                }
            }
        }

        public void FreeMemory()
        {
            lock (lockUsers)
            {
                var newCacheData = new Dictionary<string, UserWithLock>();
                foreach (var u in cacheData)
                    newCacheData.Add(u.Key, u.Value);

                cacheData = newCacheData;
            }
        }
    }
}