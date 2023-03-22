using System.Collections.Generic;

namespace MtgaDecksPro.Cards.BootstrapCardsBuilding.Entity.Mtga
{
    public record MtgaDataCardsRootObject
    {
        public int GrpId { get; set; }
        public int TitleId { get; set; }

        public bool IsToken { get; set; }
        public bool IsRebalanced { get; set; }

        public bool IsSecondaryCard { get; set; }

        public string Power { get; set; }

        public string Toughness { get; set; }
        public int FlavorId { get; set; }
        public string CollectorNumber { get; set; }

        public int Rarity { get; set; }
        public string ArtistCredit { get; set; }
        public string Set { get; set; }
        //public int linkedFaceType { get; set; }

        public string Colors { get; set; }
        public string ColorIdentity { get; set; }

        public string LinkedFaces { get; set; }

        public bool IsDigitalOnly { get; set; }
    }

    public record MtgaDataCardsRootObjectExtended : MtgaDataCardsRootObject
    {
        public string Name { get; set; }
        public string Flavor { get; set; }
    }
}