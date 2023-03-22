using System.Collections.Generic;

namespace MTGAHelper.Lib.Scraping.DeckSources.Aetherhub
{
    public class Metadeck
    {
        public int id { get; set; }
        public string name { get; set; }
        public string username { get; set; }
        public int userid { get; set; }
        public string type { get; set; }
        public int typeid { get; set; }
        public List<object> tags { get; set; }
        public List<int> color { get; set; }
        public List<int> rarity { get; set; }
        public int raritysort { get; set; }
        public int price { get; set; }
        public int likes { get; set; }
        public int views { get; set; }

        //public int comments { get; set; }
        public string comments { get; set; }

        public int popularity { get; set; }
        public long updated { get; set; }
        public long created { get; set; }
        public long updatedhidden { get; set; }
        public string tcgpartner { get; set; }
        public bool iscontentcreator { get; set; }
        public bool longdescription { get; set; }
        public bool youtubevideo { get; set; }
    }

    public class AetherhubDecklistResponse
    {
        public int draw { get; set; }
        public int recordsTotal { get; set; }
        public int recordsFiltered { get; set; }
        public List<Metadeck> metadecks { get; set; }
    }
}