using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MtgaDecksPro.Cards.BootstrapCardsBuilding.Entity.Scryfall
{
    public record CardFace
    {
        public string @object { get; set; }
        public string name { get; set; }
        public string mana_cost { get; set; }
        public string type_line { get; set; }
        public string oracle_text { get; set; }
        public List<object> colors { get; set; }
        public string flavor_text { get; set; }
        public string artist { get; set; }
        public string illustration_id { get; set; }
        public ImageUris? image_uris { get; set; }
    }

    public struct AllPart
    {
        public string @object { get; set; }
        public string id { get; set; }
        public string component { get; set; }
        public string name { get; set; }
        public string type_line { get; set; }
        public string uri { get; set; }
    }

    public struct ImageUris
    {
        public string small { get; set; }
        public string normal { get; set; }
        public string large { get; set; }
        public string png { get; set; }
        public string art_crop { get; set; }
        public string border_crop { get; set; }
    }

    public struct Legalities
    {
        public string standard { get; set; }
        public string future { get; set; }
        public string frontier { get; set; }
        public string modern { get; set; }
        public string legacy { get; set; }
        public string pauper { get; set; }
        public string vintage { get; set; }
        public string penny { get; set; }
        public string commander { get; set; }
        public string duel { get; set; }
        public string oldschool { get; set; }
    }

    public struct RelatedUris
    {
        public string gatherer { get; set; }
        public string tcgplayer_decks { get; set; }
        public string edhrec { get; set; }
        public string mtgtop8 { get; set; }
    }

    public record ScryfallModelRootObject
    {
        public string @object { get; set; }
        public Guid id { get; set; }
        public Guid oracle_id { get; set; }
        public List<int> multiverse_ids { get; set; }
        public int mtgo_id { get; set; }
        public int arena_id { get; set; }
        public int tcgplayer_id { get; set; }
        public string name { get; set; }
        public string lang { get; set; }
        public string released_at { get; set; }
        public string uri { get; set; }
        public string scryfall_uri { get; set; }
        public string layout { get; set; }
        public bool highres_image { get; set; }
        public ImageUris image_uris { get; set; }
        public string mana_cost { get; set; }
        public double cmc { get; set; }
        public string type_line { get; set; }
        public string oracle_text { get; set; }
        public string power { get; set; }
        public string toughness { get; set; }
        public List<string> colors { get; set; }
        public List<string> color_identity { get; set; }
        public Legalities legalities { get; set; }
        public List<string> games { get; set; }
        public bool reserved { get; set; }
        public bool foil { get; set; }
        public bool nonfoil { get; set; }
        public bool oversized { get; set; }
        public bool promo { get; set; }
        public bool reprint { get; set; }
        public string set { get; set; }

        [JsonPropertyName("set_name")]
        public string setName { get; set; }

        //public string set_name { get; set; }
        //public string set_uri { get; set; }
        public string set_search_uri { get; set; }

        public string scryfall_set_uri { get; set; }
        public string rulings_uri { get; set; }
        public string prints_search_uri { get; set; }
        public string collector_number { get; set; }
        public bool digital { get; set; }
        public string rarity { get; set; }
        public string watermark { get; set; }
        public string illustration_id { get; set; }
        public string artist { get; set; }
        public string border_color { get; set; }
        public string frame { get; set; }
        public List<string> frame_effects { get; set; }
        public bool full_art { get; set; }
        public bool story_spotlight { get; set; }
        public int edhrec_rank { get; set; }
        public RelatedUris related_uris { get; set; }

        public int mtgo_foil_id { get; set; }
        public List<CardFace> card_faces { get; set; }
        public List<AllPart> all_parts { get; set; }

        public bool variation { get; set; }

        //public string set_name { get; set; }  // Causing problem with C# property generation!
        //public string set_type { get; set; }  // Causing problem with C# property generation!
        //public string set_uri { get; set; }   // Causing problem with C# property generation!
        public string flavor_text { get; set; }

        public string card_back_id { get; set; }
        public List<string> artist_ids { get; set; }
        public bool textless { get; set; }
        public bool booster { get; set; }
    }
}