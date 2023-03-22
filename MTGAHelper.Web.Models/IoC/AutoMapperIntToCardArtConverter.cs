using AutoMapper;
using MTGAHelper.Lib.CardProviders;

namespace MTGAHelper.Web.Models.IoC
{
    public class AutoMapperIntToCardArtConverter : IValueConverter<int, string>
    {
        private readonly CardRepositoryProvider cardRepoProvider;

        public AutoMapperIntToCardArtConverter(CardRepositoryProvider cardRepoProvider)
        {
            this.cardRepoProvider = cardRepoProvider;
        }

        public string Convert(int sourceMember, ResolutionContext context)
        {
            var cards = cardRepoProvider.GetRepository();
            return cards.ContainsKey(sourceMember)
                ? cards[sourceMember].ImageArtUrl
                : Entity.Card.Unknown.ImageCardUrl;
        }
    }
}