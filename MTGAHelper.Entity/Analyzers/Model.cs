﻿using System.Collections.Generic;
using MTGAHelper.Entity.Config.Decks;

namespace MTGAHelper.Entity.Analyzers
{

    public class SimilarityGroup
    {
        public string SimilarityKey { get; set; }
        public ICollection<DeckWithSimilarity> Similars { get; set; }
    }

    public class DeckInfo
    {
        public ConfigModelDeck ConfigDeck { get; set; }
        public ICollection<DeckWithSimilarity> Similars { get; set; }
    }

    public class DeckWithSimilarity
    {
        public IDeck Deck { get; set; }
        public double Similarity { get; set; }
    }

    public class DecksAnalyzerResultByDeck
    {
        public string GroupKey { get; set; }

        public ICollection<DeckInfo> Decks { get; set; }
        public ICollection<DecksAnalyzerResultByCard> Cards { get; set; }
    }

    public class DecksAnalyzerResultByCard
    {
        public Card Card { get; set; }
        public ICollection<IDeck> Decks { get; set; }
    }

    public class DecksAnalyzerResult
    {
        public ICollection<DecksAnalyzerResultByCard> Cards { get; set; }
        public ICollection<DecksAnalyzerResultByDeck> Archetypes { get; set; }
    }
}
