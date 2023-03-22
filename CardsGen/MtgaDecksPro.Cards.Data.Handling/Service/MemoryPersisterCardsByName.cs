using Minmaxdev.Cache.Common.Service;
using Minmaxdev.Data.Persistence.Common.Service;
using MtgaDecksPro.Cards.Data.Handling.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace MtgaDecksPro.Cards.Data.Handling.Service
{
    public class MemoryPersisterCardsByName : DocumentPersisterBase<CardsByName>
    {
        private readonly ICacheHandler<CardsById> cacheCards;

        public MemoryPersisterCardsByName(
            DocumentPersisterConfiguration<CardsByName> configuration,
            ICacheHandler<CardsById> cacheCards
            )
            : base(configuration)
        {
            this.cacheCards = cacheCards;
        }

        public override async Task<CardsByName> Load()
        {
            return new CardsByName((await cacheCards.Get()).Values
                .GroupBy(i => i.Name)
                .ToDictionary(i => i.Key, i => i.ToList())
            );
        }
    }
}