using System.Collections.Generic;
using System.Linq;
using MTGAHelper.Entity;
using MTGAHelper.Entity.Config.App;
using MTGAHelper.Lib.CardProviders;
using MTGAHelper.Lib.Config;

namespace MTGAHelper.Lib.Analyzers
{
    public enum ArchetypeEnum
    {
        RedAggro,
        EsperControl,
        AzoriusControl,

    }

    public class ArchetypeIdentifier
    {
        private readonly ConfigModelApp configApp;
        private readonly ICardRepository cardRepo;

        public ArchetypeIdentifier(ConfigModelApp configApp, ICardRepository cardRepo)
        {
            this.configApp = configApp;
            this.cardRepo = cardRepo;
        }

        public void Identify(ICollection<int> grpIds)
        {
            var lastSet = configApp.InfoBySet.Keys.Last();

            var cards = grpIds
                .Select(i => cardRepo[i])
                .ToArray();
        }
    }
}
