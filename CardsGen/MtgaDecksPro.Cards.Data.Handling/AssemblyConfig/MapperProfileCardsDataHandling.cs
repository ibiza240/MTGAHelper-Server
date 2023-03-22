using AutoMapper;
using MtgaDecksPro.Cards.Data.Handling.AssemblyConfig.Mappers;
using MtgaDecksPro.Cards.Data.Handling.Entity;
using MtgaDecksPro.Cards.Entity;
using System.Collections.Generic;

namespace MtgaDecksPro.Cards.Data.Handling.AssemblyConfig
{
    public class MapperProfileCardsDataHandling : Profile
    {
        public MapperProfileCardsDataHandling()
        {
            CreateMap<List<Card>, CardsById>().ConvertUsing<MapperCardsById>();
            CreateMap<FormatsFileModel, FormatsAndBans>().ConvertUsing<MapperFormatsAndBans>();
        }
    }
}