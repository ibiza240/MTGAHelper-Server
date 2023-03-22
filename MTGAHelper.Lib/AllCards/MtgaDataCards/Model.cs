using MTGAHelper.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace MTGAHelper.Lib.AllCards.MtgaDataCards
{
    public class Ability
    {
        public int abilityId { get; set; }
        public int textId { get; set; }
    }

    public class MtgaDataCardsRootObject
    {
        public int grpid { get; set; }
        public int titleId { get; set; }
        public bool isToken { get; set; }
        public bool isCollectible { get; set; }

        //public bool isCraftable { get; set; }
        public int artSize { get; set; }

        public int power { get; set; }
        public int toughness { get; set; }
        public int flavorId { get; set; }
        public string CollectorNumber { get; set; }
        public int cmc { get; set; }
        public int rarity { get; set; }
        public string artistCredit { get; set; }
        public string set { get; set; }

        //public string artUrl { get; set; }
        //public object artMaskUrl { get; set; }
        public enumLinkedFace linkedFaceType { get; set; }

        public List<int> types { get; set; }
        public List<int> subtypes { get; set; }
        public List<object> supertypes { get; set; }
        public int cardTypeTextId { get; set; }
        public int subtypeTextId { get; set; }
        public List<int> colors { get; set; }
        public List<int> frameColors { get; set; }
        public List<object> frameDetails { get; set; }
        public List<int> colorIdentity { get; set; }
        public List<Ability> abilities { get; set; }
        public List<object> hiddenAbilities { get; set; }
        public List<object> linkedFaces { get; set; }
        public string castingcost { get; set; }
    }

    public class MtgaDataCardsRootObjectExtended : MtgaDataCardsRootObject
    {
        public string Name { get; set; }
    }
}