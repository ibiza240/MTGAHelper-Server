using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace MTGAHelper.Lib.Scraping.DraftHelper.MtgaZone.Spreadsheet
{
    public class MtgaZoneRatingsScraperModel
    {
        [Index(0)]
        public string CardName { get; set; }

        [Index(1)]
        public string RatingDrifter { get; set; }

        [Index(2)]
        public string RatingCompulsion { get; set; }
    }
}