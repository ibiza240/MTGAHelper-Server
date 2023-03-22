using MtgaDecksPro.Cards.Entity;

namespace MtgaDecksPro.Cards.BootstrapCardsBuilding.Entity.Scryfall
{
    public record ScryfallSetRootObject
    {
        public string @object { get; init; }
        public bool has_more { get; init; }
        public SetScryfall[] data { get; init; }
    }
}