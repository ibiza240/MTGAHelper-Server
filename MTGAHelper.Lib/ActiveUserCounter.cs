using MTGAHelper.Lib.Config.Users;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MTGAHelper.Lib
{
    public class ActiveUserCounter
    {
        private readonly object lockDequeue = new object();
        private readonly ConcurrentQueue<KeyValuePair<string, DateTime>> activeTimestampByUserId = new ConcurrentQueue<KeyValuePair<string, DateTime>>();
        private readonly ConcurrentQueue<KeyValuePair<string, DateTime>> activeDataTimestampByUserId = new ConcurrentQueue<KeyValuePair<string, DateTime>>();

        private readonly IConfigManagerUsers configManagerUsers;
        private readonly IClearUserCache cacheClearer;

        public ActiveUserCounter(IConfigManagerUsers configManagerUsers, IClearUserCache cacheClearer)
        {
            this.configManagerUsers = configManagerUsers;
            this.cacheClearer = cacheClearer;

            Task.Factory.StartNew(async () =>
            {
                int iPurge = 0;
                int iFreeMemory = 0;
                while (true)
                {
                    // Act every 15 seconds
                    await Task.Delay(15000);

                    try
                    {
                        HideInactive();

                        // Purge data every minute
                        iPurge = (iPurge + 1) % 4;

                        // Free the memory every 10 minutes
                        iFreeMemory = (iFreeMemory + 1) % 40;
                        var doFreeMemory = iFreeMemory == 0;

                        if (iPurge == 0)
                            // Purge users without activity in 20 mins
                            PurgeData(doFreeMemory, 1200);

                        //// Purge all users
                        //PurgeData(doFreeMemory, 0);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Error in ActiveUserCounter loop");
                    }
                }
            });
        }

        public void SetActive(string userId)
        {
            activeTimestampByUserId.Enqueue(new KeyValuePair<string, DateTime>(userId, DateTime.UtcNow));
            activeDataTimestampByUserId.Enqueue(new KeyValuePair<string, DateTime>(userId, DateTime.UtcNow));
        }

        private void HideInactive()
        {
            if (activeTimestampByUserId.IsEmpty)
                return;

            var threshold = DateTime.UtcNow.AddMinutes(-15);
            lock (lockDequeue)
            {
                while (activeTimestampByUserId.TryPeek(out var kvp) && kvp.Value < threshold)
                {
                    activeTimestampByUserId.TryDequeue(out _);
                }
            }
        }

        public void PurgeData(bool doFreeMemory, int nbSecondsInactive)
        {
            if (activeDataTimestampByUserId.IsEmpty)
                return;

            var threshold = DateTime.UtcNow.AddSeconds(-nbSecondsInactive);
            var userIdsToPurge = new List<string>();
            lock (lockDequeue)
            {
                while (activeDataTimestampByUserId.TryPeek(out var kvp) && kvp.Value < threshold)
                {
                    activeDataTimestampByUserId.TryDequeue(out var kvp2);

                    if (activeDataTimestampByUserId.Any(i => i.Key == kvp2.Key) == false)
                        userIdsToPurge.Add(kvp2.Key);
                }
            }

            foreach (var userId in userIdsToPurge.Distinct())
            {
                Log.Information("Purging data for {userId}", userId);

                configManagerUsers.Remove(userId);

                cacheClearer.ClearCacheForUser(userId);
            }

            if (!doFreeMemory)
                return;

            Log.Information("Freeing memory");
            configManagerUsers.FreeMemory();
            cacheClearer.FreeMemory();
        }

        public ICollection<string> GetActiveUserIds()
        {
            return activeTimestampByUserId
                .Select(i => i.Key)
                .Distinct()
                .OrderBy(i => i)
                .ToArray();
        }
    }
}