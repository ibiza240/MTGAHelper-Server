using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace MTGAHelper.Lib.Scraping.DraftHelper.InfiniteMythicEdition
{
    public class InfiniteMythicEditionRatingsModel
    {
        [Index(0)]
        public string Rating1 { get; set; }

        [Index(1)]
        public string Rating2 { get; set; }

        [Index(2)]
        public string Rating3 { get; set; }

        [Index(3)]
        public string CardName { get; set; }

        [Index(4)]
        public string InColor { get; set; }

        [Index(5)]
        public string Sideboard { get; set; }
    }
}
