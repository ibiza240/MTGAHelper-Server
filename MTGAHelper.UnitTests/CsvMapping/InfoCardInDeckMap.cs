using CsvHelper.Configuration;
using MTGAHelper.Entity;
using System.Globalization;

namespace MTGAHelper.UnitTests.CsvMapping
{
    public sealed class InfoCardInDeckMap : ClassMap<InfoCardInDeck>
    {
        public InfoCardInDeckMap()
        {
            AutoMap(CultureInfo.InvariantCulture);
        }
    }

}
