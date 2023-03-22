﻿namespace MTGAHelper.Entity
{
    public enum DeckCardZoneEnum
    {
        Deck,
        Sideboard,
        Commander,
        Companion,
    };

    public interface IDeckCard
    {
        //bool IsSideboard { get; }
        //int NbMissing { get; }
        //float MissingWeight { get; }
        string DisplayMember { get; }
        DeckCardZoneEnum Zone { get; }
    }

    public class DeckCard : CardWithAmount, IDeckCard
    {
        public DeckCardZoneEnum Zone { get; set; }
        //public bool IsSideboard => Zone == DeckCardZoneEnum.Sideboard;

        public new string DisplayMember
        {
            get
            {
                var typeChar = Card.GetSimpleType()[0];
                //return $"[{typeChar}] {Amount}x {Card.name} (Missing {NbMissing})";
                return $"[{typeChar}] {Amount}x {Card.Name}";
            }
        }

        public DeckCard()
        {
            // Required for serialization
        }

        public DeckCard(Card card, int amount, DeckCardZoneEnum zone)
            : base(card, amount)
        {
            Zone = zone;
        }

        //public DeckCard(CardWithAmount card, bool isSideboard)
        //    : base(card.Card, card.Amount)
        //{
        //    //IsSideboard = isSideboard;
        //    Zone = isSideboard ? DeckCardZoneEnum.Deck : DeckCardZoneEnum.Sideboard;
        //}

        //public int NbMissing { get; private set; }

        //public float MissingWeight { get; private set; }

        //public void ApplyCompareResult(int nbMissing, float missingWeight)
        //{
        //    NbMissing = nbMissing;
        //    MissingWeight = missingWeight;
        //}
    }
}
