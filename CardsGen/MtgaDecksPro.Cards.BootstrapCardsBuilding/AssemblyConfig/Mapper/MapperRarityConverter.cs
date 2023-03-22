using AutoMapper;
using MtgaDecksPro.Cards.Entity;
using System;

namespace MtgaDecksPro.Cards.BootstrapCardsBuilding.AssemblyConfig.Mapper
{
    public class MapperRarityConverter : IValueConverter<int, string>
    {
        public string Convert(int source, ResolutionContext context)
        {
            switch (source)
            {
                case 0: // Token
                case 1: // Basic Land
                case 2:
                    return Constants.Common;

                case 3:
                    return Constants.Uncommon;

                case 4:
                    return Constants.Rare;

                case 5:
                    return Constants.Mythic;
            }

            throw new NotSupportedException("Invalid rarity");
        }
    }
}