using AutoMapper;
using MTGAHelper.Entity;
using MTGAHelper.Lib.CardProviders;

namespace MTGAHelper.Lib.IoC
{
    public class AutoMapperGrpIdToCardConverter : ITypeConverter<int, Card>
    {
        readonly CardRepositoryProvider cardRepoProvider;

        public AutoMapperGrpIdToCardConverter(CardRepositoryProvider cardRepoProvider)
        {
            this.cardRepoProvider = cardRepoProvider;
        }

        public Card Convert(int source, Card destination, ResolutionContext context)
        {
            var cardRepo = cardRepoProvider.GetRepository();
            return cardRepo.GetValueOrDefault(source, Card.Unknown);
        }
    }
}
