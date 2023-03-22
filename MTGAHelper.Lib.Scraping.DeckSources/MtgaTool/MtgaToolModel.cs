using System;
using System.Collections.Generic;
using System.Text;

namespace MTGAHelper.Lib.Scraping.DeckSources.MtgaTool
{
    public class DeckCards
    {
        public int quantity { get; set; }
        public int id { get; set; }
    }

    public class BestDeck
    {
        public List<DeckCards> mainDeck { get; set; }
        public List<DeckCards> sideboard { get; set; }
        public List<DeckCards> arenaMain { get; set; }
        public List<DeckCards> arenaSide { get; set; }
        public string name { get; set; }
        public int deckTileId { get; set; }
        public List<int> colors { get; set; }
        public List<int> commandZoneGRPIds { get; set; }
        public string format { get; set; }
        public string type { get; set; }
        public string description { get; set; }
        public string archetype { get; set; }
        public string owner { get; set; }
    }

    public class Best
    {
        public string name { get; set; }
        public double dev { get; set; }
        public double high { get; set; }
        public string closest { get; set; }
    }

    public class Deck
    {
        public double wr { get; set; }
        public int wrt { get; set; }
        public string name { get; set; }
        public string owner { get; set; }
        public string match { get; set; }
        public List<int> colors { get; set; }
        public Best best { get; set; }
    }

    public class Meta
    {
        public int total { get; set; }
        public List<int> colors { get; set; }
        public int tile { get; set; }
        public int win { get; set; }
        public int loss { get; set; }
        public BestDeck best_deck { get; set; }
        public double best_deck_wr { get; set; }
        public int best_deck_wrt { get; set; }
        public List<Deck> decks { get; set; }
        public string share { get; set; }
        public string winrate { get; set; }
        public string name { get; set; }
    }

    public class MtgaToolModel
    {
        public string _id { get; set; }
        public DateTime date { get; set; }
        public DateTime date_start { get; set; }
        public int days { get; set; }
        public string format { get; set; }
        public List<Meta> meta { get; set; }
    }
}