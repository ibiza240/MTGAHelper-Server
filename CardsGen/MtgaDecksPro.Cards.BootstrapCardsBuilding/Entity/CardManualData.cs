using MtgaDecksPro.Cards.Entity;

namespace MtgaDecksPro.Cards.BootstrapCardsBuilding.Entity
{
    public record CardManualData
    {
        public string TypeLine { get; init; }
        public string ManaCost { get; init; }
        public string OracleText { get; init; }
        public string SetScryfall { get; init; }

        public string ImageCardUrl { get; init; } = Constants.UnknownCardImage;
        public string ImageArtUrl { get; init; } = Constants.UnknownCardImage;
    }
}