using AutoMapper;
using MTGAHelper.Entity;
using MTGAHelper.Entity.Config.Decks;
using MTGAHelper.Lib.Config;
using MTGAHelper.Lib.Scraping.CalendarScraper;
using MTGAHelper.Lib.UserStats;

namespace MTGAHelper.Lib.IoC
{
    public class MapperProfileLib : Profile
    {
        public MapperProfileLib(UtilColors utilColors)
        {
            //CreateMap<IImmutableUser, UserInfo>(MemberList.None);

            CreateMap<string, TimeframeEnum>()
                .ConvertUsing(i =>
                    i == "d" ? TimeframeEnum.Daily :
                    i == "h" ? TimeframeEnum.Hourly :
                    TimeframeEnum.Unknown
                );

            CreateMap<ConfigModelDeck, DeckSummary>()
                .ForMember(m => m.DateCreated, o => o.MapFrom(x => x.DateCreatedUtc))
                .ForMember(m => m.Color, o => o.ConvertUsing(utilColors, x => x.Deck))
                .ForMember(m => m.Hash, o => o.Ignore())
            ;
            CreateMap<ConfigModelDeck, DeckTrackedSummary>(MemberList.None)
                .ForMember(i => i.DateCreated, i => i.MapFrom(x => x.DateCreatedUtc))
                .ForMember(i => i.Color, i => i.ConvertUsing(utilColors, x => x.Deck))
            ;

            CreateMap<CalendarScraperMtgaAssistantItemModel, ConfigModelCalendarItem>()
                .ForMember(i => i.Title, i => i.MapFrom(x => x.Title.Replace("’", "'")))
                .ForMember(i => i.Description, i => i.MapFrom(x => x.Description.Replace("’", "'")))
                .ForMember(i => i.DateRange, i => i.MapFrom(x => x.Date))
                .ForMember(i => i.Image, i => i.Ignore())
                .ForMember(i => i.ImageAetherhub, i => i.MapFrom(x => x.Image));

            //new DeckSummary
            //{
            //    Id = configDeck.Id,
            //    Name = configDeck.Deck.Name,
            //    DateCreated = configDeck?.DateCreatedUtc,
            //    MissingWeight = missingWeight,
            //    MissingWeightBase = missingWeight / priorityFactor,
            //    PriorityFactor = priorityFactor,
            //    ScraperTypeId = configDeck.ScraperTypeId,
            //    Url = configDeck?.Url,
            //    Color = configDeck.Deck.GetColor(),
            //    WildcardsMissingMain = userData.CompareResult.GetWildcardsMissing(configDeck.Id, false, false),
            //    WildcardsMissingSideboard = userData.CompareResult.GetWildcardsMissing(configDeck.Id, true, false),
            //}

            CreateMap<Card, CardLandPreferenceDto>()
                .ForMember(m => m.IsSelected, o => o.Ignore())
                .ForMember(i => i.Name, i => i.MapFrom(x => x.Name))
                .ForMember(i => i.ImageCardUrl, i => i.MapFrom(x => x.ImageCardUrl))
                .ForMember(i => i.GrpId, i => i.MapFrom(x => x.GrpId));

            //CreateMap<GetDeckListResultDeckRaw, GetDeckListResultDeck>()
            //    .ForMember(i => i.MainDeck, i => i.Ignore())
            //    .ForMember(i => i.Sideboard, i => i.Ignore());

            //CreateMap<GetCombinedRankInfoRaw, GetCombinedRankInfoResult>();

            //CreateMap<List<DeckCardRaw>, Dictionary<int, int>>().ConvertUsing(i => i.ToDictionary(x => Convert.ToInt32(x.id), x => Convert.ToInt32(x.quantity)));

            //CreateMap<CourseDeckRaw, ConfigModelRawDeck>()
            //    //.ForMember(i => i.Name, i => i.MapFrom(x => x.name))
            //    .ForMember(i => i.CardsMain, i => i.MapFrom(x => x.mainDeck))
            //    .ForMember(i => i.CardsSideboard, i => i.MapFrom(x => x.sideboard));

            //CreateMap<InfoByDate<DateSnapshot>, HistorySummaryForDate>() // todo this removal ok?
            //    .ForMember(i => i.Date, i => i.MapFrom(x => x.DateTime.Date))
            //    .ForMember(i => i.OutcomesByMode, i => i.MapFrom(x => x.Info.Info.OutcomesByMode))
            //    .ForMember(i => i.ConstructedRank, i => i.MapFrom(x => x.Info.Info.RankSynthetic.FirstOrDefault(y => y.Format == RankFormatEnum.Constructed) ?? new ConfigModelRankInfo(RankFormatEnum.Constructed)))
            //    .ForMember(i => i.LimitedRank, i => i.MapFrom(x => x.Info.Info.RankSynthetic.FirstOrDefault(y => y.Format == RankFormatEnum.Limited) ?? new ConfigModelRankInfo(RankFormatEnum.Limited)))
            //    .ForMember(i => i.GemsChange, i => i.MapFrom(x => x.Info.Diff.GemsChange))
            //    .ForMember(i => i.GoldChange, i => i.MapFrom(x => x.Info.Diff.GoldChange))
            //    .ForMember(i => i.VaultProgressChange, i => i.MapFrom(x => x.Info.Diff.VaultProgressChange))
            //    .ForMember(i => i.WildcardsChange, i => i.MapFrom(x => x.Info.Diff.WildcardsChange))
            //    .ForMember(i => i.NewCardsCount, i => i.MapFrom(x => x.Info.Diff.NewCards.Sum(y => y.Value)));

            //CreateMap<MtgaDataCardsRootObject, Card>()
            //    .ForMember(i => i.set, i => i.MapFrom(x => x.set == "MI" ? "MIR" : x.set == "PS" ? "PLS" : x.set.ToUpper().Replace("DAR", "DOM")))
            //    .ForMember(i => i.number, i => i.MapFrom(x => x.CollectorNumber));

            //CreateMap<ScryfallModelRootObject, Card>()
            //    .ForMember(i => i.name, i => i.MapFrom(x => x.layout == "transform" ? x.name.Split('/')[0].Trim() : x.name))
            //    .ForMember(i => i.grpId, i => i.MapFrom(x => x.arena_id))
            //    .ForMember(i => i.set, i => i.MapFrom(x => x.set.ToUpper().Replace("DAR", "DOM")))
            //    .ForMember(i => i.number, i => i.MapFrom(x => x.collector_number))
            //    .ForMember(i => i.type, i => i.MapFrom(x => x.layout == "transform" ? x.type_line.Split('/')[0].Trim() : x.type_line))
            //    .ForMember(i => i.imageCardUrl, i => i.MapFrom(x => (x.layout == "transform" ? x.card_faces[0].image_uris.border_crop : x.image_uris.border_crop).Replace("https://img.scryfall.com/cards", "")))
            //    .ForMember(i => i.imageArtUrl, i => i.MapFrom(x => (x.layout == "transform" ? x.card_faces[0].image_uris.art_crop : x.image_uris.art_crop).Replace("https://img.scryfall.com/cards", "")))
            //    .ForMember(i => i.colors, i => i.MapFrom(x => x.color_identity.Select(y => y.ToUpper())));
        }
    }
}
