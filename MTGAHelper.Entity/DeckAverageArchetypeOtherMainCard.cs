﻿using System;
using System.Linq;

namespace MTGAHelper.Entity
{
    public class DeckAverageArchetypeOtherMainCard : IDeckCard
    {
        public Card Card { get; }
        public DeckCardZoneEnum Zone => DeckCardZoneEnum.Deck;
        public int NbMissing => 0;
        public float MissingWeight => 0.0f;

        public DeckAverageArchetypeOtherMainCard(Card card, int nbOwned)
        //:base(card)
        {
            Card = card;
            throw new Exception("REIMPLEMENT CARD CONSTRUCTOR");
        }

        public int NbOwned { get; private set; }

        public string DisplayMember
        {
            get
            {
                var typeChars = "L";

                if (Card.Type.Contains("Land") == false)
                {
                    var t = Card.Type.Replace("Legendary", "").Trim();
                    var typeWords = t.Contains("—") ? t.Substring(0, t.IndexOf("—")).Trim() : t;
                    typeChars = string.Join("", typeWords.Split(' ').Select(i => i[0]));
                }

                return $"[{typeChars}] {Card.Name} ({NbOwned} owned)";
            }
        }
        public void ApplyCompareResult(int nbOwned)
        {
            NbOwned = nbOwned;
        }
    }
}
