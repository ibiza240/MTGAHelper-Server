using MTGAHelper.Entity;
using MTGAHelper.Lib.CardProviders;
using System.Linq;

namespace MTGAHelper.Lib.Scraping.DraftHelper.MtgaZone.Spreadsheet
{
    public class MtgaZoneRatingsScraperSpreadsheet : MtgaZoneRatingsScraperBase
    {
        public MtgaZoneRatingsScraperSpreadsheet(string folderData, ICardRepository allCards)
            : base(new MtgaZoneRatingsScraperBaseArgs
            {
                FolderData = folderData,
                AllCards = allCards
            })
        {
        }

        private readonly string[] sets = { "ONE", "BRO", "DMU", "HBG", "SNC", "NEO", "M21", "IKO", "ZNR", "KLR", "STX", "AFR", "MID", "VOW" };

        public DraftRatings Scrape(string setFilter = "")
        {
            var ret = new DraftRatings();

            foreach (var set in sets.Where(i => setFilter == "" || i == setFilter))
            {
                var ratings = GetRatingsForSet(set);
                ret.RatingsBySet.Add(set, ratings);
            }

            return ret;
        }
    }
}