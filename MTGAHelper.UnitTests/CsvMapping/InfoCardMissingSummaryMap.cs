using CsvHelper.Configuration;
using MTGAHelper.Entity;
using System.Globalization;

namespace MTGAHelper.UnitTests.CsvMapping
{
    public sealed class InfoCardMissingSummaryMap : ClassMap<InfoCardMissingSummary>
    {
        public InfoCardMissingSummaryMap()
        {
            AutoMap(CultureInfo.InvariantCulture);
        }
    }
}
