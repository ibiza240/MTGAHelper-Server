using System.Collections.Generic;

namespace MTGAHelper.Lib.Scraping.DeckSources.Streamdecker
{
    public class Data
    {
        public List<Deck> decks { get; set; }
        public int total { get; set; }
        public int limit { get; set; }
        public int next { get; set; }
        public UserProfile userProfile { get; set; }
    }

    public class RootObjectDecksList
    {
        public string status { get; set; }
        public Data data { get; set; }
        public string message { get; set; }
    }

    public class RootObjectLatest
    {
        public string status { get; set; }
        public List<Deck> data { get; set; }
        public string message { get; set; }
    }

    public class Views
    {
        public int counter { get; set; }
        public object updated { get; set; }
    }

    public class UserProfile
    {
        public string logo { get; set; }
        public string displayName { get; set; }
        public string name { get; set; }
        public string profileBanner { get; set; }
        public object profileBannerBackgroundColor { get; set; }
        public string twitchId { get; set; }
    }

    public class ColorIdentity
    {
        public bool G { get; set; }
        public bool R { get; set; }
        public bool B { get; set; }
        public bool U { get; set; }
        public bool W { get; set; }
    }

    //public class Cardhoarder
    //{
    //    public int id { get; set; }
    //    public double price { get; set; }
    //}

    //public class Manatraders
    //{
    //    public int id { get; set; }
    //    public double price { get; set; }
    //}

    //public class Mtgotraders
    //{
    //    public object id { get; set; }
    //    public double price { get; set; }
    //}

    //public class Cardkingdom
    //{
    //    public int id { get; set; }
    //    public double price { get; set; }
    //}

    //public class Price
    //{
    //    public Cardhoarder cardhoarder { get; set; }
    //    public Manatraders manatraders { get; set; }
    //    public Mtgotraders mtgotraders { get; set; }
    //    public Cardkingdom cardkingdom { get; set; }
    //}

    public class Mtga
    {
        public string name { get; set; }
        public string set { get; set; }
        public string setId { get; set; }
        public string rarity { get; set; }
    }

    public class CardList
    {
        public string name { get; set; }
        public int cmc { get; set; }
        public List<string> colorIdentity { get; set; }
        public List<string> types { get; set; }
        public string manaCost { get; set; }
        public object manaSplitCost { get; set; }
        public string scryfallId { get; set; }
        public string layout { get; set; }
        public int main { get; set; }
        public int sideboard { get; set; }
        public int commander { get; set; }
        public int companion { get; set; }
        public List<string> manaSource { get; set; }
        public Mtga mtga { get; set; }
        public string text { get; set; }
    }

    public class Deck
    {
        public string name { get; set; }
        public Views views { get; set; }
        public UserProfile userProfile { get; set; }
        public int twitchId { get; set; }
        public string deckLink { get; set; }
        public ColorIdentity colorIdentity { get; set; }
        public string createdAt { get; set; }
        public string updatedAt { get; set; }
        public string visibility { get; set; }
        public List<string> featuredCards { get; set; }
        public List<string> featuredScyfallIdCards { get; set; }
        public List<CardList> cardList { get; set; }
    }

    public class RootObjectDeck
    {
        public string status { get; set; }
        public Deck data { get; set; }
        public object message { get; set; }
    }
}