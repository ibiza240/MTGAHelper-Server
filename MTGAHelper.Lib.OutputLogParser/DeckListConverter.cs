using System.Collections.Generic;
using AutoMapper;
using MTGAHelper.Entity;
using MTGAHelper.Lib.CardProviders;
using Serilog;

namespace MTGAHelper.Lib.OutputLogParser
{
    public class DeckListConverter : IValueConverter<IList<int>, Dictionary<int, int>>
    {
        private readonly CardRepositoryProvider cardRepoProvider;

        public DeckListConverter(CardRepositoryProvider cardRepoProvider)
        {
            this.cardRepoProvider = cardRepoProvider;
        }

        public ICollection<CardWithAmount> ConvertCards(IList<int> cardsInfo)
        {
            var cardRepo = cardRepoProvider.GetRepository(); 
            var cards = new List<CardWithAmount>();
            var iCard = 0;
            while (iCard < cardsInfo.Count)
            {
                var grpId = cardsInfo[iCard];
                var amount = cardsInfo[iCard + 1];
                cards.Add(new CardWithAmount(cardRepo[grpId], amount));
                iCard += 2;
            }

            return cards;
        }

        public Dictionary<int, int> ConvertSimple(IList<int> cardsInfo)
        {
            var cards = new Dictionary<int, int>();

            if (cardsInfo == default)
                return cards;

            var iCard = 0;
            while (iCard < cardsInfo.Count)
            {
                var grpId = cardsInfo[iCard];
                var amount = cardsInfo[iCard + 1];

                if (cards.ContainsKey(grpId))
                {
                    Log.Error("STRANGE! grpId {grpId} was found multiple times in DeckListConverter.ConvertSimple with cardsInfo: <cardsInfo>", grpId, string.Join(",", cardsInfo));
                    cards[grpId] += amount;
                }
                else
                    cards.Add(grpId, amount);

                iCard += 2;
            }

            return cards;
        }

        public Dictionary<int, int> Convert(IList<int> sourceMember, ResolutionContext context)
        {
            return ConvertSimple(sourceMember);
        }
    }
}