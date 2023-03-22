using System.Threading.Tasks;

namespace MtgaDecksPro.Tools.Internal.Service
{
    public enum ChoiceDispatcherEnum
    {
        Unknown,
        BuildCards,
        BuildCardsInDecks,
        DownloadMtgaAssets,
        DownloadScryfallCards,
    }

    public class ChoiceDispatcher
    {
        private readonly CommandBuildSetsAndCards choiceDispatchCardsBuilder;
        private readonly CommandDownloadMtgaAssets choiceDispatchDownloadMtgaAssets;
        private readonly CommandDownloadScryfallCards choiceDispatchDownloadScryfallCards;

        public ChoiceDispatcher(
            CommandBuildSetsAndCards choiceDispatchCardsBuilder,
            CommandDownloadMtgaAssets choiceDispatchDownloadMtgaAssets,
            CommandDownloadScryfallCards choiceDispatchDownloadScryfallCards
            )
        {
            this.choiceDispatchCardsBuilder = choiceDispatchCardsBuilder;
            this.choiceDispatchDownloadMtgaAssets = choiceDispatchDownloadMtgaAssets;
            this.choiceDispatchDownloadScryfallCards = choiceDispatchDownloadScryfallCards;
        }

        public async Task ProcessChoice(ChoiceDispatcherEnum choice)
        {
            switch (choice)
            {
                case ChoiceDispatcherEnum.BuildCards:
                    await choiceDispatchCardsBuilder.Execute();
                    break;

                case ChoiceDispatcherEnum.DownloadMtgaAssets:
                    await choiceDispatchDownloadMtgaAssets.Execute();
                    break;

                case ChoiceDispatcherEnum.DownloadScryfallCards:
                    await choiceDispatchDownloadScryfallCards.Execute();
                    break;
            }
        }
    }
}