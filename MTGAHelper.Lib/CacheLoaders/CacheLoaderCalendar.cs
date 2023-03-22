using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using MTGAHelper.Entity;
using MTGAHelper.Entity.Config.App;
using MTGAHelper.Lib.CardProviders;
using MTGAHelper.Lib.Config;
using Newtonsoft.Json;

namespace MTGAHelper.Lib.CacheLoaders
{
    public class CacheLoaderCalendar : ICacheLoader<IReadOnlyCollection<ConfigModelCalendarItem>>
    {
        private readonly IDataPath dataPath;

        public CacheLoaderCalendar(
            IDataPath dataPath
            )
        {
            this.dataPath = dataPath;
        }

        public IReadOnlyCollection<ConfigModelCalendarItem> LoadData()
        {
            var data = Array.Empty<ConfigModelCalendarItem>();
            var file = Path.Join(dataPath.FolderData, "calendar.json");

            if (File.Exists(file))
            {
                var content = File.ReadAllText(file);
                data = JsonConvert.DeserializeObject<ConfigModelCalendarItem[]>(content);
            }

            return data;
        }
    }

    public class CacheCalendarImageBinder
    {
        private readonly CacheSingleton<IReadOnlyCollection<ConfigModelCalendarItem>> cacheCalendar;
        private readonly CacheLoaderCalendar cacheLoaderCalendar;
        private readonly ICardRepository cardRepository;
        private readonly string folderData;

        public CacheCalendarImageBinder(
            CacheSingleton<IReadOnlyCollection<ConfigModelCalendarItem>> cacheCalendar,
            CacheLoaderCalendar cacheLoaderCalendar,
            ICardRepository cardRepository,
            IDataPath dataPath
            )
        {
            this.cacheCalendar = cacheCalendar;
            this.cacheLoaderCalendar = cacheLoaderCalendar;
            this.cardRepository = cardRepository;
            this.folderData = dataPath.FolderData;
        }

        public void Reload()
        {
            var latestCache = cacheLoaderCalendar.LoadData()
                .Select(i =>
                {
                    (DateTime dateEnding, long timestampEnding) GetDateInfo()
                    {
                        DateTime dateEnding = default;
                        var timestampEnding = 0L;

                        var stringParts = i.DateRange.Split(" to ").Select(i => i.Trim()).ToArray();
                        if (stringParts.Length == 1)
                        {
                            var monthEnding = stringParts[0];
                            var monthParts = monthEnding.Split(" ");
                            DateTime.TryParse($"{monthParts[1]} {monthParts[0]} {DateTime.UtcNow.Year}", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateEnding);
                            timestampEnding = dateEnding.Ticks;
                        }
                        else
                        {
                            var monthEnding = stringParts[1];
                            var monthParts = monthEnding.Split(" ");
                            if (monthParts.Length == 1 && int.TryParse(monthEnding, out _))
                            {
                                monthParts = new[] { stringParts[0].Split(" ")[0], monthEnding };
                                i.DateRange = stringParts[0] + " to " + $"{monthParts[0]} {monthParts[1]}";
                            }

                            if (DateTime.TryParse($"{monthParts[1]} {monthParts[0]} {DateTime.UtcNow.Year}", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateEnding))
                                timestampEnding = dateEnding.Ticks;
                            else
                                timestampEnding = long.MaxValue;
                        }

                        return (dateEnding, timestampEnding);
                    };

                    var dateInfo = GetDateInfo();
                    return new { item = i, dateInfo.timestampEnding, dateInfo.dateEnding };
                })
                .Where(i => i.dateEnding >= DateTime.UtcNow.AddDays(-1))
                .OrderBy(i => i.timestampEnding)
                .Select(i => i.item)
                .ToArray();

            string fileContents = File.ReadAllText(Path.Join(folderData, "calendarImages.json"));
            var knownImages = JsonConvert.DeserializeObject<ConfigModelCalendarImages>(fileContents)!;
            foreach (var cal in latestCache.Where(i => knownImages.ImageBySet.ContainsKey(i.Title)))
            {
                var cardName = knownImages.ImageBySet[cal.Title];
                var candidates = cardRepository.CardsByName(cardName);
                var card = knownImages.ImageSetByTitle.TryGetValue(cal.Title, out string set)
                    ? candidates.FirstOrDefault(c => c.Set == set)
                    : candidates.FirstOrDefault();

                if (card != default && card.Name != "Unknown")
                    cal.Image = card.ImageArtUrl;
            }

            cacheCalendar.Set(latestCache);
        }
    }
}
