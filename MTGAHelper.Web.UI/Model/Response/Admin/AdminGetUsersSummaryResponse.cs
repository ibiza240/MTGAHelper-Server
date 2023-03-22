using MTGAHelper.Lib.IO.Reader;
using MTGAHelper.Lib.UserStats;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MTGAHelper.Web.UI.Model.Response.Admin
{
    public class AdminGetUsersSummaryResponse
    {
        public string ServerUptime { get; set; }
        public int NbRecurrentUsers { get; set; }
        public ICollection<UsersSliceInfoDto> Summary { get; set; }


        public AdminGetUsersSummaryResponse(ICollection<UserHistoryInfo> details, ICollection<UserHistoryInfo> allHistory, DateTime timestampServerCreatedUtc)
        {
            Summary = details
                .GroupBy(i => i.TimestampUtc)
                .Select(slice => new UsersSliceInfoDto(slice.Key, slice, allHistory)).ToArray();

            var dateActiveUserUtc = DateTime.UtcNow.Date.AddDays(-5);
            NbRecurrentUsers = details
                .Where(i => i.Type == UserHistoryInfoTypeEnum.History)
                .GroupBy(i => i.UserId)
                .Where(i => i.Any(x => x.TimestampUtc >= dateActiveUserUtc))
                .Where(i => i.Count() > 1)
                .Count();

            var hoursRunning = (DateTime.UtcNow - timestampServerCreatedUtc).TotalHours;
            ServerUptime = $"{hoursRunning.ToString("#,0.00")} hours";
        }
    }

    public class UsersSliceInfoDto
    {
        public DateTime TimestampUtc { get; set; }
        public int NbNewUsers { get; set; }
        public int NbNewUploads { get; set; }
        public float SuccessRateNewUsers { get; set; }
        public int NbUploadsForRecurrentUser { get; set; }
        public int NbPageViews { get; set; }

        public UsersSliceInfoDto(DateTime sliceDateUtc, IEnumerable<UserHistoryInfo> sliceData, ICollection<UserHistoryInfo> allHistory)
        {
            TimestampUtc = sliceDateUtc;

            NbNewUsers = sliceData
                .Where(i => i.Type == UserHistoryInfoTypeEnum.ConfigCreated)
                .Count();

            NbPageViews = sliceData
                .Where(i => i.Type == UserHistoryInfoTypeEnum.ConfigUpdated)
                .Count();

            var uploadsInfo = sliceData
                .Where(i => i.Type == UserHistoryInfoTypeEnum.History)
                .GroupBy(i => i.UserId);

            NbNewUploads = uploadsInfo
                .Where(i => allHistory.Where(x => x.UserId == i.Key).Count() == 1)
                .Count();

            NbUploadsForRecurrentUser = uploadsInfo
                .Where(i => allHistory.Where(x => x.UserId == i.Key).Count() > 1)
                .Sum(i => i.Count());

            SuccessRateNewUsers = NbNewUsers == 0 ? 0f : (float)NbNewUploads / NbNewUsers * 100;
        }
    }
}
