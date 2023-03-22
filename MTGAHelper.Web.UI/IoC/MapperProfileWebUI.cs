using AutoMapper;
using MTGAHelper.Entity;
using MTGAHelper.Lib.Config;
using MTGAHelper.Lib.MasteryPass;
using MTGAHelper.Web.Models.Response.Misc;
using MTGAHelper.Web.Models.Response.User;
using System.Collections.Generic;

namespace MTGAHelper.Web.UI.IoC
{
    public class MapperProfileWebUI : Profile
    {
        public MapperProfileWebUI(
            MapperConverterLimitedResultsSummary mapperConverterLimitedResultsSummary
            )
        {
            CreateMap<MasteryPassCalculator, UserMasteryPassResponse>()
                .ForMember(i => i.DateEnd, i => i.MapFrom(x => x.masteryPass.DateEndUtc.ToString("yyyy-MM-dd")))
                .ForMember(i => i.CurrentLevel, i => i.MapFrom(x => x.inputs.CurrentLevel))
                .ForMember(i => i.CurrentXp, i => i.MapFrom(x => x.inputs.CurrentXp))
                .ForMember(i => i.DailyQuestsAvailable, i => i.MapFrom(x => x.inputs.DailyQuestsAvailable))
                .ForMember(i => i.DailyWinsCompleted, i => i.MapFrom(x => x.inputs.DailyWinsCompleted))
                .ForMember(i => i.WeeklyWinsCompleted, i => i.MapFrom(x => x.inputs.WeeklyWinsCompleted));

            CreateMap<LimitedEventResults, UserStatsLimitedResponseEvent>();

            CreateMap<ICollection<LimitedEventResults>, UserStatsLimitedResponse>()
                .ForMember(i => i.Summary, i => i.ConvertUsing(mapperConverterLimitedResultsSummary, x => x))
                .ForMember(i => i.Details, i => i.MapFrom(x => x));

            CreateMap<ConfigModelCalendarItem, GetCalendarResponse>();
        }
    }
}