namespace MtgaDecksPro.Cards.Entity
{
    public record SetScryfall
    {
        public string? @object { get; init; }
        public string? id { get; init; }
        public string? code { get; init; }
        public int tcgplayer_id { get; init; }
        public string? name { get; init; }
        public string? uri { get; init; }
        public string? scryfall_uri { get; init; }
        public string? search_uri { get; init; }
        public string? released_at { get; init; }
        public string? set_type { get; init; }
        public int card_count { get; init; }
        public bool digital { get; init; }
        public bool nonfoil_only { get; init; }
        public bool foil_only { get; init; }
        public string? icon_svg_uri { get; init; }
        public string? parent_set_code { get; init; }
        public string? mtgo_code { get; init; }
        public string? arena_code { get; init; }
        public string? block_code { get; init; }
        public string? block { get; init; }
    }
}