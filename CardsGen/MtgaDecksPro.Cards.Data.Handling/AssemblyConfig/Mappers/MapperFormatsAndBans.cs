using AutoMapper;
using Minmaxdev.Cache.Common.Service;
using MtgaDecksPro.Cards.Entity;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MtgaDecksPro.Cards.Data.Handling.AssemblyConfig.Mappers
{
    internal class MapperFormatsAndBans : ITypeConverter<FormatsFileModel, FormatsAndBans>
    {
        private readonly ILogger logger;
        private readonly ICacheHandler<List<HistoricAnthologyCards>> cacheHistoricAnthology;

        public MapperFormatsAndBans(
            ILogger logger,
            ICacheHandler<List<HistoricAnthologyCards>> cacheHistoricAnthology
            )
        {
            this.logger = logger;
            this.cacheHistoricAnthology = cacheHistoricAnthology;
        }

        public FormatsAndBans Convert(FormatsFileModel source, FormatsAndBans destination, ResolutionContext context)
        {
            foreach (var invalidFormat in source.Where(i => FormatEnum.IsValid(i.Key) == false))
                logger.Warning("Unsupported Format found in formats.json: <{invalidFormat}>", invalidFormat.Key);

            var historicAnthologies = cacheHistoricAnthology.Get().Result;

            (string format, int bestOf, BanEnum banType, string cardName)[] flattenedFormats = source
                .SelectMany(format => (
                    format.Value.Banned?.Select(cardName => (format.Key, format.Value.BestOf, BanEnum.Banned, cardName)) ?? Array.Empty<(string, int, BanEnum, string)>())
                        .Union(format.Value.Suspended?.Select(cardName => (format.Key, format.Value.BestOf, BanEnum.Suspended, cardName)) ?? Array.Empty<(string, int, BanEnum, string)>())
                )
                .ToArray();

            var formatsAndBans = new FormatsAndBans
            {
                HistoricAdditionalCards = historicAnthologies
                    .ToDictionary(i => Enum.Parse<AdditionalCardsEnum>($"HistoricAnthology{i.Id}"), i => i.Cards),

                FormatInfo = source
                    .Where(i => i.Value.Obsolete == false)
                    .ToDictionary(i => i.Key, i =>
                    {
                        //if (i.Value.Sets.First() == "Alchemy")
                        //    System.Diagnostics.Debugger.Break();

                        return new FormatInfo
                        {
                            Name = i.Key,
                            Description = i.Value.Description,
                            IsActive = i.Value.Active,
                            BestOf = i.Value.BestOf,
                            CardPoolType = Enum.TryParse(i.Value.Sets.FirstOrDefault(), false, out CardPoolEnum cp) ? cp : CardPoolEnum.Sets,
                            CardPool = i.Value.Sets,
                            CardsBanned =
                                (i.Value.Banned?.Select(cardName => new CardBanned { BanType = BanEnum.Banned, Name = cardName }) ?? Array.Empty<CardBanned>()).Union
                                (i.Value.Suspended?.Select(cardName => new CardBanned { BanType = BanEnum.Suspended, Name = cardName }) ?? Array.Empty<CardBanned>()
                            ).ToArray()
                        };
                    }),

                BansByCard = flattenedFormats
                    .GroupBy(i => i.cardName)
                    .ToDictionary(i => i.Key, i => i.Select(x => new CardBanned { Name = x.format, BanType = x.banType }).ToArray()),
            };

            return formatsAndBans;
        }
    }
}