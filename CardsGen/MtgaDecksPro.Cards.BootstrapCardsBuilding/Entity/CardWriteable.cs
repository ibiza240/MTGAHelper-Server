using Minmaxdev.Common;
using MtgaDecksPro.Cards.Entity;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MtgaDecksPro.Cards.BootstrapCardsBuilding.Entity
{
    [Serializable]
    public record CardWriteable : IRouteId, IName
    {
        public string IdComputed => IdArena == default ? IdScryfall.GetHashCode().ToString() : IdArena.ToString();

        // MTGA

        public Guid IdScryfall { get; set; }
        public int IdArena { get; set; }
        public string Name { get; set; }
        public bool IsToken { get; set; }
        public bool IsRebalanced { get; set; }
        public bool IsPrimaryCard { get; set; }
        public int Power { get; set; }
        public int Toughness { get; set; }
        public string Flavor { get; set; }
        public string Number { get; set; }  // MTGA: CollectorNumber    MUST BE STRING (e.g. Golgari Queen: GR8)
        public int Cmc { get; set; }
        public string Rarity { get; set; }
        public string ArtistCredit { get; set; }
        public string SetArena { get; set; }
        public ICollection<char> Colors { get; set; } = new char[0];
        public ICollection<char> ColorIdentity { get; set; } = new char[0];

        //public LinkedFaceEnum LinkedFaceType { get; set; }
        public ICollection<int> LinkedFaces { get; set; } = new int[0];

        // Scryfall

        public string TypeLine { get; set; } = "N/A";
        public string Layout { get; set; } = "N/A";
        public string ManaCost { get; set; } = "N/A";
        public string OracleText { get; set; } = "N/A";
        public string SetScryfall { get; set; } = "N/A";
        public string ImageCardUrl { get; set; } = Cards.Entity.Constants.UnknownCardImage;
        public string ImageArtUrl { get; set; } = Cards.Entity.Constants.UnknownCardImage;
        //public string SetFullName { get; set; }
        //public DateOnly ReleaseDate { get; set; }
        //public string ImageCardExtendedArtUrl { get; set; }

        // Custom

        public bool IsStyle { get; set; }
        public bool IsInBooster { get; set; }
        public ICollection<int> RelatedTokenCards { get; set; }

        public RarityEnum? RarityEnum => string.IsNullOrEmpty(Rarity) ? (RarityEnum?)null : Enum.Parse<RarityEnum>(Rarity, true);
        public ICollection<string> RouteId => new string[] { Name, IdArena.ToString() };
        public bool IsCollectible { get; set; }
        public bool IsCraftable { get; set; }
        public bool IsMultiCard => LayoutListMultiCard.Any(i => i == Layout);

        public CardMatchMethodEnum BuilderMethod { get; set; }

        private static readonly ICollection<string> LayoutListMultiCard = new[]
        {
            "modal_dfc",
            "adventure",
            "split",
            "transform",
        };
    }
}