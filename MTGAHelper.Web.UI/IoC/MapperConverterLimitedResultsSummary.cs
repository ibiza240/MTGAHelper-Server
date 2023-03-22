using AutoMapper;
using MTGAHelper.Entity;
using MTGAHelper.Web.Models.Response.User;
using System.Collections.Generic;
using System.Linq;

namespace MTGAHelper.Web.UI.IoC
{
    public class MapperConverterLimitedResultsSummary : IValueConverter<ICollection<LimitedEventResults>, ICollection<UserStatsLimitedResponseSummary>>
    {
        public MapperConverterLimitedResultsSummary()
        {
        }

        public ICollection<UserStatsLimitedResponseSummary> Convert(ICollection<LimitedEventResults> source, ResolutionContext context)
        {
            var resultsByEvent = source
                .GroupBy(i => (i.Set, i.DraftType))
                .Select(i =>
                {
                    return new UserStatsLimitedResponseSummary
                    {
                        Set = i.Key.Set,
                        DraftType = i.Key.DraftType,
                        EventsCount = i.Count(),
                        WinCountAverage = (float)i.Average(x => x.WinCount),
                        WinRate = (float)i.Sum(i => i.WinCount) / i.Sum(i => i.TotalCount),
                    };
                }).ToArray();

            return resultsByEvent;
        }
    }
}