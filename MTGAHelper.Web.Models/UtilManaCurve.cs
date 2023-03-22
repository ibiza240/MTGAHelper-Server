﻿using MTGAHelper.Entity;
using MTGAHelper.Web.Models.Response.Deck;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MTGAHelper.Web.Models
{
    public class UtilManaCurve
    {
        public ICollection<DeckManaCurveDto> CalculateManaCurve(ICollection<CardWithAmount> cards)
        {
            if ((cards?.Any() ?? false) == false)
                return Array.Empty<DeckManaCurveDto>();

            var manaInfo = cards
                .Where(i => i.Card.Type.Contains("Land") == false)
                .GroupBy(i => Math.Min(7, i.Card.Cmc))
                .ToDictionary(i => i.Key, i => i);

            //if (manaInfo.Values.Any() == false)
            //    System.Diagnostics.Debugger.Break();

            var maxCardsForMana = manaInfo.Any() ? manaInfo.Values.Max(i => i.Sum(x => x.Amount)) : 1;

            var manaCurve = new[] { 0, 1, 2, 3, 4, 5, 6, 7 }
                .Select(i =>
                {
                    var nbCards = manaInfo.ContainsKey(i) ? manaInfo[i].Sum(x => x.Amount) : 0;

                    return
                        new DeckManaCurveDto
                        {
                            ManaCost = i,
                            NbCards = nbCards,
                            PctHeight = nbCards * 100 / maxCardsForMana
                        };
                })
                .ToArray();

            return manaCurve;
        }
    }
}