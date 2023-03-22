using AutoMapper;
using MTGAHelper.Entity;
using MTGAHelper.Lib.CardProviders;
using System.Collections.Generic;
using System.Linq;

namespace MTGAHelper.Lib
{
    public class UtilColors : IValueConverter<ICollection<int>, string>, IValueConverter<IDeck, string>
    {
        readonly Dictionary<string, int> order = new()
        {
            { "W", 1 },
            { "U", 2 },
            { "B", 3 },
            { "R", 4 },
            { "G", 5 },
        };

        readonly CardRepositoryProvider cardRepoProvider;

        public UtilColors(CardRepositoryProvider cardRepositoryProvider)
        {
            cardRepoProvider = cardRepositoryProvider;
        }

        public string FromGrpIds(IEnumerable<int> grpIds)
        {
            var allCards = cardRepoProvider.GetRepository();
            IEnumerable<Card> cards = grpIds
                .Select(i => allCards.GetValueOrDefault(i))
                .Where(i => i is not null);
            return FromCards(cards);
        }

        public string FromDeck(IDeck deck)
        {
            return FromCards(deck.Cards.All.Select(i => i.Card));
        }

        public string FromCards(IEnumerable<Card> cards)
        {
            var colors = GetColorFromCards(cards);
            return string.Join("", colors);
        }

        IEnumerable<string> GetColorFromCards(IEnumerable<Card> cards)
        {
            var (lands, nonLands) = cards.SplitBy(i => i.Type.Contains("Land"));
            var landsColors = lands
                .Where(i => i.ColorIdentity != null)
                .SelectMany(i => i.ColorIdentity)
                .Distinct();

            var colors = nonLands
                .Where(i => i.ColorIdentity != null)
                .SelectMany(i => i.ColorIdentity)
                .Distinct()
                .Where(i => landsColors.Contains(i))
                .OrderBy(i => order[i]);

            return colors;
        }

        public string Convert(ICollection<int> sourceMember, ResolutionContext context)
        {
            return FromGrpIds(sourceMember);
        }

        public string Convert(IDeck sourceMember, ResolutionContext context)
        {
            return FromDeck(sourceMember);
        }
    }
}
