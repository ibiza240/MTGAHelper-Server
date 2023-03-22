using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MTGAHelper.Entity;
using MTGAHelper.Lib.CardProviders;

namespace MTGAHelper.Lib.Scraping.DraftHelper
{
    public class SharedTools
    {
        private readonly ICardRepository allCards;

        public SharedTools(ICardRepository cardRepo)
        {
            this.allCards = cardRepo;
        }

        public Dictionary<string, ICollection<DraftRatingTopCard>> GetTop5CommonByColor(ICollection<DraftRating> ratings)
        {
            var toFix = ratings.Where(i => allCards.CardsByName(i.CardName).Any() == false).ToArray();
            if (toFix.Any()) Debugger.Break();

            var byColor = ratings
                .Where(i => allCards.CardsByName(i.CardName).FirstOrDefault() != null)
                .Select(i =>
                {
                    return new
                    {
                        i.CardName,
                        rating = i,
                        card = allCards.CardsByName(i.CardName).First()
                    };
                })
                .Select(i => new
                {
                    i.CardName,
                    i.rating,
                    color = i.card.Colors.Count == 1 ? i.card.Colors.First().Substring(0, 1).ToUpper() : null,
                    rarity = i.card.Rarity,
                })
                .Where(i => i.color != null && i.rarity == RarityEnum.Common)
                .OrderByDescending(i => i.rating.RatingValue)
                .GroupBy(i => i.color, i => i)
                .ToDictionary(
                    i => i.Key,
                    i => (ICollection<DraftRatingTopCard>)i.Take(5)
                        .Select((x, index) => new DraftRatingTopCard(index + 1, x.CardName))
                        .ToArray());

            return byColor;
        }
    }
}