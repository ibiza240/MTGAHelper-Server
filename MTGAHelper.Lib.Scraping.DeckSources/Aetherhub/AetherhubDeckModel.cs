using System;
using System.Collections.Generic;
using System.Text;

namespace MTGAHelper.Lib.Scraping.DeckSources.Aetherhub
{
    public class AetherhubDeckModel
    {
        public ICollection<AetherhubDeckCardModel> convertedDeck { get; set; }
    }

    public class AetherhubDeckCardModel
    {
        public int? quantity { get; set; }
        public string name { get; set; }
        public string set { get; set; }
        public string number { get; set; }
    }
}