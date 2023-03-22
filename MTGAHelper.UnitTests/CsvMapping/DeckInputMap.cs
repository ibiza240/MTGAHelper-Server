using CsvHelper.Configuration;
using MTGAHelper.Entity;
using System.Globalization;

namespace MTGAHelper.UnitTests.CsvMapping
{
    public sealed class DeckInputMap : ClassMap<DeckCardRawModel>
    {
        public DeckInputMap()
        {
            AutoMap(CultureInfo.InvariantCulture);
        }
    }
}
