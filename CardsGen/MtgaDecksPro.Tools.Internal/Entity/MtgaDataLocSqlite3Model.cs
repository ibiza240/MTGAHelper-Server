using CsvHelper.Configuration.Attributes;

namespace MtgaDecksPro.Tools.Internal
{
    internal class MtgaDataLocSqlite3Model
    {
        public int LocId { get; set; }
        public bool Formatted { get; set; }
        public string enUS { get; set; }
    }

    internal class MtgaDataCardSqlite3Model
    {
        public int GrpId { get; set; }
        public int TitleId { get; set; }

        public bool IsToken { get; set; }
        public bool IsRebalanced { get; set; }

        public bool IsPrimaryCard { get; set; }

        public string Power { get; set; }

        public string Toughness { get; set; }
        public string CollectorNumber { get; set; }

        public int Rarity { get; set; }
        public string ArtistCredit { get; set; }
        public string ExpansionCode { get; set; }
        public string DigitalReleaseSet { get; set; }
        public int LinkedFaceType { get; set; }

        public string Colors { get; set; }
        public string ColorIdentity { get; set; }

        public bool IsDigitalOnly { get; set; }

        ///////
        ///
        public bool IsSecondaryCard => !IsPrimaryCard;

        [Ignore]
        public string Set { get; set; }
    }
}