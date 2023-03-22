using AutoMapper;
using MtgaDecksPro.Cards.Data.Handling.Entity;
using MtgaDecksPro.Cards.Entity;
using System.Collections.Generic;
using System.Linq;

namespace MtgaDecksPro.Cards.Data.Handling.AssemblyConfig.Mappers
{
    internal class MapperCardsById : ITypeConverter<List<Card>, CardsById>
    {
        public CardsById Convert(List<Card> source, CardsById destination, ResolutionContext context)
        {
            return new CardsById(source
                .GroupBy(i => i.IdComputed)
                .ToDictionary(i => i.Key, i => i.Single())
            );
        }
    }
}