using System;

namespace MTGAHelper.Lib.Scraping.DeckSources
{
    public class DeckScraperDeckInputs
    {
        public string UrlDownloadDeck { get; set; }
        public string UrlViewDeck { get; set; }
        public string UrlDeckList { get; set; }
        public string Name { get; set; }
        public DateTime DateCreated { get; set; }
        public int VariantId { get; set; }
        public int? OrderIndex { get; set; }
        public string DeckText { get; set; }

        public DeckScraperDeckInputs(string name)
        {
            Name = name;
        }

        public DeckScraperDeckInputs(string name, DateTime dateCreated)
        {
            Name = name;
            DateCreated = dateCreated;
        }
    }
}