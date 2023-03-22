﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace MTGAHelper.Entity
{
    public interface IDeck
    {
        string Name { get; set; }
        ScraperType ScraperType { get; }
        DeckCards Cards { get; }

        string Id { get; }

        bool FilterName(string filter);
        bool FilterColor(string filter);
        bool FilterSource(string source, string filter);
        bool FilterCard(string filter);
    }

    public abstract class DeckBase : IDeck
    {
        protected DeckBase(string name, ScraperType scraperType)
        {
            Name = name;
            ScraperType = scraperType;
        }

        public string Name { get; set; }
        public ScraperType ScraperType { get; protected set; }

        public DeckCards Cards { get; protected set; }

        public string Id { get; protected set; }

        public string GetId()
        {
            var cardsMain = Cards.QuickCardsMain.Values.GroupBy(i => i.Card.Name).Select(i => $"{i.Sum(x => x.Amount)} {i.Key}");
            var cardsSideboard = Cards.QuickCardsSideboard.Values.GroupBy(i => i.Card.Name).Select(i => $"{i.Sum(x => x.Amount)} {i.Key}");

            var m = string.Join("_", cardsMain);
            var s = string.Join("_", cardsSideboard);
            var id = Fnv1aHasher.To32BitFnv1aHash($"{m}|{s}");

            return $"{ScraperType}_{id}";
        }

        public bool FilterSource(string scraperTypeId, string filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
                return true;

            return
                scraperTypeId.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0
                //|| filter == "(All)"
                //|| (filter == "(Unknown)" && string.IsNullOrWhiteSpace(source))
                ;
        }

        public bool FilterColor(string filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
                return true;

            var colors = GetColorListFromString(filter);

            if (colors.Count == 0)
                return true;

            if (colors.Any(filterColor => !Cards.ColorsInDeck.Contains(filterColor)))
            {
                // quickly filter decks that don't have this color at all
                return false;
            }

            if (colors.Count == 1)
            {
                // Mono...we check for at least 80% cards of that single color
                var minNbCards = (int)(Cards.All.Count * 0.8f);
                var color = colors.Single();
                var nbCards = Cards.All
                    .Count(i => i.Card.Colors == null || i.Card.Colors.Count == 1 && i.Card.Colors.Single() == color);
                return nbCards >= minNbCards;
            }

            // multicolor, look for exactly these colors
            return Cards.All
                .Where(i => i.Card.Colors != null)
                .Where(i => i.Zone != DeckCardZoneEnum.Sideboard) //.Where(i => i.IsSideboard == false)
                .All(i => i.Card.Colors.All(c => colors.Contains(c)));
        }

        private static List<string> GetColorListFromString(string filter)
        {
            var filterUpperCase = filter.ToUpper();

            var colors = new List<string>(5);

            if (filterUpperCase.Contains("W"))
                colors.Add("W");
            if (filterUpperCase.Contains("U"))
                colors.Add("U");
            if (filterUpperCase.Contains("B"))
                colors.Add("B");
            if (filterUpperCase.Contains("R"))
                colors.Add("R");
            if (filterUpperCase.Contains("G"))
                colors.Add("G");
            return colors;
        }

        public bool FilterName(string filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
                return true;

            return Name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public bool FilterCard(string filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
                return true;

            return Cards.All.Any(i => i.Card.Name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0);
        }
    }

    public class Deck : DeckBase
    {
        public Deck(string name, ScraperType scraperTypeId, IEnumerable<DeckCard> cards)
            : base(name, scraperTypeId)
        {
            Cards = new DeckCards(cards);
            Id = GetId();

            //if (Id == "aetherhub-user_mtgarenaoriginaldecks-arenastandard_2891280397") Debugger.Break();
        }

        //public override float CalcMissingWeight() { return Cards.Sum(i => i.MissingWeight); }

        //public override void ApplyCompareResult(Card card, bool isSideboard, int nbMissing, float missingWeight, int nbOwned)
        //{
        //    var c = Cards.Single(i => i.Card == card && i.IsSideboard == isSideboard);
        //    c.ApplyCompareResult(nbMissing, missingWeight);
        //}
    }
}
