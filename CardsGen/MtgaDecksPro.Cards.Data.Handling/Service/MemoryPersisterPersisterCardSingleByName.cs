using Minmaxdev.Cache.Common.Service;
using Minmaxdev.Data.Persistence.Common.Service;
using MtgaDecksPro.Cards.Data.Handling.Entity;
using MtgaDecksPro.Cards.Entity;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MtgaDecksPro.Cards.Data.Handling.Service
{
    public class MemoryPersisterPersisterCardSingleByName : DocumentPersisterBase<CardSingleByName, List<Card>>
    {
        private ICacheHandler<CardsById> cacheCards;

        public MemoryPersisterPersisterCardSingleByName(
            DocumentPersisterConfiguration<CardSingleByName> configuration,
            ICacheHandler<CardsById> cachedDataHandlerCards
            )
            : base(configuration)
        {
            this.cacheCards = cachedDataHandlerCards;
        }

        public override async Task<CardSingleByName> Load()
        {
            return new CardSingleByName((await cacheCards.Get()).Values
                .GroupBy(i => i.Name)
                .ToDictionary(i => i.Key, i => i.First())
                );
        }
    }
}