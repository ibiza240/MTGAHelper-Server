using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace MTGAHelper.Entity
{
    public class Card //: IEquatable<Card>
    {
        private const string UNKNOWN_IMAGE =
            "https://cdn11.bigcommerce.com/s-0kvv9/images/stencil/1280x1280/products/266486/371622/classicmtgsleeves__43072.1532006814.jpg?c=2&imbypass=on";
        public static Card Unknown { get; } = new Card(0, "Unknown");

        public int GrpId { get; }
        public string Name { get; }
        public string Type { get; }
        public string RarityStr { get; }
        public RarityEnum Rarity { get; }
        public string ManaCost { get; }
        public string Set { get; }
        public string Number { get; }
        public bool IsFoundInBooster { get; }
        public bool NotInBooster => !IsFoundInBooster;
        public string ImageCardUrl { get; }
        public string ImageArtUrl { get; }
        public int Cmc { get; }
        public ICollection<string> Colors { get; }
        public ICollection<string> ColorIdentity { get; }

        // From data_cards.mtga
        public enumLinkedFace LinkedFaceType { get; }

        /// <summary>
        /// If applicable, the card face that would be found in the library or graveyard,
        /// or the other face of the card, if the current face would be found in the library.
        /// For split cards: only the halves (which are not found in the library) have this value set.
        /// </summary>
        public int LinkedCardGrpId { get; }

        public Card LinkedCard { get; private set; }

        public bool IsToken { get; }
        public bool IsCollectible { get; }
        public bool IsStyle { get; }
        public bool IsRebalanced { get; }
        public string ArtistCredit { get; }

        private Card(int grpId, string name, string type = null, string rarity = null,
            ICollection<string> colors = null, ICollection<string> colorIdentity = null, string imageCardUrl = null)
        {
            GrpId = grpId;
            Name = name;
            Type = type ?? string.Empty;
            RarityStr = rarity;
            Rarity = ParseRarityEnum(RarityStr);
            ImageCardUrl = imageCardUrl ?? UNKNOWN_IMAGE;
            Colors = colors ?? Array.Empty<string>();
            ColorIdentity = colorIdentity ?? Array.Empty<string>();
        }

        public Card(Card2 card)
        : this(card.IdArena, card.Name, card.TypeLine, card.Rarity, card.Colors, card.ColorIdentity, card.imageCardUrl)
        {
            ManaCost = card.ManaCost;
            Set = card.SetScryfall.ToUpper();
            Number = card.Number;
            IsFoundInBooster = card.IsInBooster;
            ImageArtUrl = card.imageArtUrl;
            Cmc = card.Cmc;
            LinkedFaceType = card.LinkedFaceType;
            LinkedCardGrpId = card.LinkedFaces?.Count == 1 ? card.LinkedFaces.First() : 0;
            IsToken = card.IsToken;
            IsCollectible = card.IsCollectible;
            IsStyle = card.IsStyle;
            IsRebalanced = card.IsRebalanced;
            ArtistCredit = card.Artist;
        }

        [JsonConstructor]// for use in ConsoleSync (maybe elsewhere?)
        public Card( int grpId, string name, string type, string rarity,
            ICollection<string> colors, ICollection<string> colorIdentity, string imageCardUrl,
            string manaCost, string set, string number, bool isFoundInBooster, string imageArtUrl, int cmc,
            enumLinkedFace linkedFaceType, int linkedCardGrpId, bool isToken, bool isCollectible, string artistCredit)
            : this(grpId, name, type, rarity, colors, colorIdentity, imageCardUrl)
        {
            ManaCost = manaCost;
            Set = set;
            Number = number;
            IsFoundInBooster = isFoundInBooster;
            ImageArtUrl = imageArtUrl;
            Cmc = cmc;
            LinkedFaceType = linkedFaceType;
            LinkedCardGrpId = linkedCardGrpId;
            IsToken = isToken;
            IsCollectible = isCollectible;
            ArtistCredit = artistCredit;
        }

        public void SetLinkedCard(Card card)
        {
            LinkedCard ??= card;
        }

        public string GetSimpleType()
        {
            if (Type.Contains("Land"))
                return "Land";

            var t = Type.Replace("Legendary", "");

            var dashPosition = t.IndexOf("—", StringComparison.Ordinal);
            var typeWords = dashPosition < 0
                ? t
                : t.Substring(0, dashPosition);

            var slashPosition = typeWords.IndexOf("//", StringComparison.Ordinal);
            var firstHalf = slashPosition < 0
                ? typeWords
                : typeWords.Substring(0,slashPosition);

            return firstHalf.Trim();
        }

        public RarityEnum GetRarityEnumSplitRareLands()
        {
            if (Rarity != RarityEnum.Rare)
                return Rarity;

            return Type.ToLower().Contains("land")
                ? RarityEnum.RareLand
                : RarityEnum.RareNonLand;
        }

        public bool DoesProduceManaOfColor(string color)
        {
            return ThisFaceProducesManaOfColor(color) || LinkedFaceType == enumLinkedFace.MDFC_Front && (LinkedCard?.ThisFaceProducesManaOfColor(color) == true);
        }

        public bool ThisFaceProducesManaOfColor(string color)
        {
            return Type.Contains("Land") && (color == "C" ? ColorIdentity.Count == 0 : ColorIdentity.Contains(color));
        }

        public bool CompareNameWith(string n)
        {
            if (Name == n)
            {
                return true;
            }
            // To match cards with variants ("Card name (a)")
            //return name.Length > 4 && name.Substring(0, name.Length - 4) == n;
            return false;
        }


        //public static bool operator ==(Card c1, Card c2)
        //{
        //    if (c1 is null || c2 is null)
        //    {
        //        return c1 is null && c2 is null;
        //    }

        //    return c1.Equals(c2);
        //}

        //public static bool operator !=(Card c1, Card c2)
        //{
        //    return !(c1 == c2);
        //}

        //public bool Equals(Card other)
        //{
        //    if (other is null)
        //    {
        //        return false;
        //    }

        //    if (ReferenceEquals(this, other))
        //        return true;
        //    else
        //        return CompareNameWith(other.name);
        //}

        public override int GetHashCode()
        {
            return -191684997 + EqualityComparer<string>.Default.GetHashCode(Name);
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override string ToString()
        {
            return Name;
        }

        
        private static RarityEnum ParseRarityEnum(string rarity)
        {
            if (string.IsNullOrWhiteSpace(rarity))
                return RarityEnum.Unknown;

            // Because different sources of cards...
            // Scryfall: "Mythic Rare"
            // MTGATool: "mythic"
            var r = rarity.Split(' ')[0].ToLower();

            return r switch
            {
                "mythic" => RarityEnum.Mythic,
                "rare" => RarityEnum.Rare,
                "uncommon" => RarityEnum.Uncommon,
                "common" => RarityEnum.Common,
                _ => RarityEnum.Unknown
            };
        }
    }
}