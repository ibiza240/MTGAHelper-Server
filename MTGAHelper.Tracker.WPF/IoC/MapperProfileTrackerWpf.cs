using AutoMapper;
using MTGAHelper.Tracker.WPF.Models;
using MTGAHelper.Tracker.WPF.ViewModels;
using MTGAHelper.Web.Models.Response.User;
using MTGAHelper.Tracker.WPF.Config;
using System.Collections.Generic;

namespace MTGAHelper.Tracker.WPF.IoC
{
    public class MapperProfileTrackerWpf : Profile
    {
        public MapperProfileTrackerWpf(BorderGradientCalculator gradientCalculator)
        {
            CreateMap<CardForDraftPickDto, CardDraftPickWpf>()
                .ForMember(i => i.RareDraftPickEnum, i => i.MapFrom(x => x.IsRareDraftPick))
                .ForMember(m => m.CustomRatingDescription, opt => opt.Ignore())
                .ForMember(m => m.CustomRatingValue, opt => opt.Ignore())
                .ForMember(m => m.NbMissing, opt => opt.Ignore())
                .ForMember(m => m.ArenaId, opt => opt.MapFrom(x => x.IdArena))
                .ForMember(m => m.ColorIdentity, opt => opt.Ignore())
                .ForMember(m => m.Cmc, opt => opt.Ignore())
                .ForMember(m => m.Type, opt => opt.Ignore())
                .IgnoreAllPropertiesWithAnInaccessibleSetter(); // CmcImages weirdly gets detected as writable

            CreateMap<CardDraftPickWpf, CardDraftPickVM>()
                .ForMember(i => i.BorderGradient, opt => opt.MapFrom(x => gradientCalculator.CalculateBorderGradient(x)))
                .IgnoreAllPropertiesWithAnInaccessibleSetter(); // CmcImages weirdly gets detected as writable

            CreateMap<Entity.Card, CardWpf>()
                .ForMember(i => i.ArenaId, i => i.MapFrom(x => x.GrpId))
                .ForMember(i => i.ColorIdentity, i => i.MapFrom(x => x.ColorIdentity))
                .ForMember(i => i.ManaCost, i => i.MapFrom(x => x.ManaCost))
                .IgnoreAllPropertiesWithAnInaccessibleSetter(); // CmcImages weirdly gets detected as writable

            CreateMap<CardWithAmountWpf, LibraryCardWithAmountVM>(MemberList.None);
            //.ForMember(i => i.ImageArtUrl, i => i.MapFrom(x => new Util().GetThumbnailLocal(x.ImageArtUrl)));

            CreateMap<ConfigModel, OptionsWindowVM>()
                .IgnoreAllPropertiesWithAnInaccessibleSetter()
                .ForMember(m => m.DraftRatings, opt => opt.Ignore()) // currently set "by hand"
                .ForMember(m => m.Sets, opt => opt.Ignore()) // currently set "by hand"
                .ForMember(m => m.LimitedRatingsSource, opt => opt.MapFrom(x => new KeyValuePair<string, string>(x.LimitedRatingsSource, x.LimitedRatingsSource)));
            //.ForMember(i => i.ForceCardPopupSide, i => i.MapFrom(x =>  string.IsNullOrEmpty(x.ForceCardPopupSide) ? "On the left" : x.ForceCardPopupSide));
        }
    }
}