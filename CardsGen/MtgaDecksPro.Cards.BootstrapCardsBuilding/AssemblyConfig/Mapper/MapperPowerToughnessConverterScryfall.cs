using AutoMapper;

namespace MtgaDecksPro.Cards.BootstrapCardsBuilding.AssemblyConfig.Mapper
{
    public class MapperPowerToughnessConverterScryfall : IValueConverter<string, int>
    {
        public int Convert(string source, ResolutionContext context)
        {
            if (int.TryParse(source, out int r))
                return r;
            else
                return 0;
        }
    }
}