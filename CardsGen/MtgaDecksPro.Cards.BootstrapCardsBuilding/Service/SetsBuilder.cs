using Minmaxdev.Cache.Common.Service;
using MtgaDecksPro.Cards.BootstrapCardsBuilding.Entity.Scryfall;
using MtgaDecksPro.Cards.Entity;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MtgaDecksPro.Cards.BootstrapCardsBuilding.Service
{
    public class SetsBuilder
    {
        private readonly ILogger logger;
        private readonly ICacheHandler<ScryfallSetRootObject> cacheHandlerScryfallSets;

        private Dictionary<string, string> exceptionsArenaCode = new Dictionary<string, string>
        {
            ["con"] = "conf",
        };

        public SetsBuilder(
            ILogger logger,
            ICacheHandler<ScryfallSetRootObject> cacheHandlerScryfallSets
            )
        {
            this.logger = logger;
            this.cacheHandlerScryfallSets = cacheHandlerScryfallSets;
        }

        public async Task<ICollection<SetScryfall>> GetSets()
        {
            logger.Debug("GetFullCardsAndSets()");

            var scryfallSetsRoot = await cacheHandlerScryfallSets.Get();
            if (scryfallSetsRoot.has_more)
                throw new ApplicationException("Scryfall sets list is not complete anymore!!!");

            var data = scryfallSetsRoot.data.Select(set =>
            {
                return exceptionsArenaCode.ContainsKey(set.code) ? set with { arena_code = exceptionsArenaCode[set.code] } : set;
            }).ToArray();

            return data;
        }
    }
}