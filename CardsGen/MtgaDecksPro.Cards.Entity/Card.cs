using Minmaxdev.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MtgaDecksPro.Cards.Entity
{
    [Serializable]
    public record Card : IRouteId, IName
    {
        public string IdComputed => IdArena == default ? IdScryfall.GetHashCode().ToString() : IdArena.ToString();

        // MTGA

        public Guid IdScryfall { get; init; }
        public int IdArena { get; init; }
        public string Name { get; init; }
        public bool IsToken { get; init; }
        public bool IsRebalanced { get; init; }
        public bool IsPrimaryCard { get; init; }
        public int Power { get; init; }
        public int Toughness { get; init; }
        public string Flavor { get; init; }
        public string Number { get; init; }  // MTGA: CollectorNumber    MUST BE STRING (e.g. Golgari Queen: GR8)
        public int Cmc { get; init; }
        public string Rarity { get; init; }
        public string ArtistCredit { get; init; }
        public string SetArena { get; init; }
        public ICollection<char> Colors { get; init; } = new char[0];
        public ICollection<char> ColorIdentity { get; init; } = new char[0];

        //public LinkedFaceEnum LinkedFaceType { get; init; }
        public ICollection<int> LinkedFaces { get; init; } = new int[0];

        // Scryfall

        public string TypeLine { get; init; } = "N/A";
        public string Layout { get; init; } = "N/A";
        public string ManaCost { get; init; } = "N/A";
        public string OracleText { get; init; } = "N/A";
        public string SetScryfall { get; init; } = "N/A";
        public string ImageCardUrl { get; init; } = Constants.UnknownCardImage;
        public string ImageArtUrl { get; init; } = Constants.UnknownCardImage;
        //public string ImageCardExtendedArtUrl { get; init; }

        // Custom

        public bool IsStyle { get; init; }
        public bool IsInBooster { get; init; }
        public ICollection<int> RelatedTokenCards { get; init; }

        public RarityEnum? RarityEnum => string.IsNullOrEmpty(Rarity) ? (RarityEnum?)null : Enum.Parse<RarityEnum>(Rarity, true);
        public ICollection<string> RouteId => new string[] { Name, IdArena.ToString() };
        public bool IsCollectible { get; init; }
        public bool IsCraftable { get; init; }
        public bool IsMultiCard => LayoutListMultiCard.Any(i => i == Layout);

        public CardMatchMethodEnum BuilderMethod { get; init; }

        private static readonly ICollection<string> LayoutListMultiCard = new[]
        {
            "modal_dfc",
            "adventure",
            "split",
            "transform",
        };
    }
}