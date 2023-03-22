using MTGAHelper.Lib.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MTGAHelper.Lib.UserStats
{
    public class ReaderUserHistoryInfo
    {
        //ConfigModelApp configApp;
        //IConfigManagerUsers configUsers;
        //TimeframeEnum timeframe;

        //public ReaderUserHistoryInfo(IOptionsMonitor<ConfigModelApp> configApp, IConfigManagerUsers configUsers)
        //{
        //    this.configApp = configApp.CurrentValue;
        //    this.configUsers = configUsers;
        //}

        //public ReaderUserHistoryInfo Init(TimeframeEnum timeframe)
        //{
        //    this.timeframe = timeframe;
        //    return this;
        //}

        //private DateTime trimDate(DateTime date)
        //{
        //    var roundTicks = default(long);
        //    switch (timeframe)
        //    {
        //        case TimeframeEnum.Daily:
        //            roundTicks = TimeSpan.TicksPerDay;
        //            break;
        //        case TimeframeEnum.Hourly:
        //            roundTicks = TimeSpan.TicksPerHour;
        //            break;
        //        default:
        //            return date;
        //    }

        //    return new DateTime(date.Ticks - (date.Ticks % roundTicks), date.Kind);
        //}

        //public ICollection<UserHistoryInfo> GetInfoConfig(DateTime dateThresholdUtc)
        //{
        //    var configFiles = Directory.GetFiles(configApp.FolderDataConfigUsers)
        //        .Where(i => Path.GetExtension(i) == ".json")
        //        .Select(i => new FileInfo(i));

        //    var created = configFiles
        //        .Where(i => i.CreationTimeUtc >= dateThresholdUtc)
        //        .Select(i => new UserHistoryInfo
        //        {
        //            UserId = Path.GetFileNameWithoutExtension(i.FullName.Substring(0, Math.Min(32, i.FullName.Length))),
        //            //Filepath = i.FullName,
        //            TimestampUtc = trimDate(i.CreationTimeUtc),
        //            Type = UserHistoryInfoTypeEnum.ConfigCreated,
        //        });

        //    var updated = configFiles
        //        .Where(i => i.LastWriteTimeUtc >= dateThresholdUtc)
        //        .Select(i => new UserHistoryInfo
        //        {
        //            UserId = Path.GetFileNameWithoutExtension(i.FullName.Substring(0, Math.Min(32, i.FullName.Length))),
        //            //Filepath = i.FullName,
        //            TimestampUtc = trimDate(i.CreationTimeUtc),
        //            Type = UserHistoryInfoTypeEnum.ConfigUpdated,
        //        });

        //    return created.Union(updated).ToArray();
        //}

        //public ICollection<UserHistoryInfo> GetInfoHistory(DateTime dateThresholdUtc)
        //{
        //    return Directory.GetFiles(configApp.FolderDataUserHistory, "*.json", SearchOption.AllDirectories)
        //        .Select(i => new FileInfo(i))
        //        .Where(i => i.LastWriteTimeUtc >= dateThresholdUtc)
        //        .Select(i => new UserHistoryInfo
        //        {
        //            UserId = i.Name.Substring(0, Math.Min(32, i.Name.Length)),
        //            //Filepath = i.FullName,
        //            TimestampUtc = trimDate(i.LastWriteTimeUtc),
        //            Type = UserHistoryInfoTypeEnum.History,
        //        })
        //        .ToArray();
        //}

        //public ICollection<UserHistoryInfo> GetInfoForTimeframe()
        //{
        //    if (timeframe == TimeframeEnum.Unknown)
        //        return new UserHistoryInfo[0];

        //    var dateThresholdUtc = GetThresholdUtc();

        //    var info = GetInfoHistory(dateThresholdUtc)
        //        .Union(GetInfoConfig(dateThresholdUtc))
        //        .OrderByDescending(i => i.TimestampUtc)
        //        .ToArray();

        //    return info;
        //}

        //private DateTime GetThresholdUtc()
        //{
        //    var dateNowUtc = DateTime.UtcNow;
        //    var dateRefUtc = trimDate(dateNowUtc);

        //    switch (timeframe)
        //    {
        //        case TimeframeEnum.Daily:
        //            return dateRefUtc.AddDays(-30);
        //        case TimeframeEnum.Hourly:
        //            return dateRefUtc.AddHours(-24);
        //        default:
        //            return dateRefUtc;
        //    }
        //}

        //public ICollection<UserHistoryInfo> GetInfo()
        //{
        //    var result = new List<UserHistoryInfo>();
        //    timeframe = TimeframeEnum.Daily;
        //    var dateT = GetThresholdUtc();

        //    var dateFor = dateT;
        //    while (dateT < DateTime.UtcNow)
        //    {
        //        var res = new UserHistoryInfo
        //        {
        //            TimestampUtc = dateT,
        //            Type = UserHistoryInfoTypeEnum.ConfigCreated,
        //        };
        //        result.Add(res);

        //        dateT = dateT.AddDays(1);
        //    }

        //    return result;
        //}
    }
}
