using MtgaDecksPro.Cards.BootstrapCardsBuilding.Service;
using MtgaDecksPro.Cards.Entity.Config;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace MtgaDecksPro.Tools.Internal.Service
{
    public class CommandBuildSetsAndCards : ICommand
    {
        private readonly string folderData;
        private readonly SetsBuilder setsBuilder;
        private readonly CardsBuilder cardsBuilder;

        public CommandBuildSetsAndCards(
            IConfigFolderDataCards configFolderDataCards,
            SetsBuilder setsBuilder,
            CardsBuilder cardsBuilder
            )
        {
            this.folderData = configFolderDataCards.FolderDataCards;
            this.setsBuilder = setsBuilder;
            this.cardsBuilder = cardsBuilder;
        }

        public async Task Execute()
        {
            var sets = await setsBuilder.GetSets();
            await File.WriteAllTextAsync($"{folderData}/sets.json", JsonSerializer.Serialize(sets));

            var cards = await cardsBuilder.GetFullCards();
            await File.WriteAllTextAsync($"{folderData}/cards.json", JsonSerializer.Serialize(cards));
        }
    }
}