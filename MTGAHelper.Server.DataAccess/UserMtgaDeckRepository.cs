using MTGAHelper.Entity;
using MTGAHelper.Server.DataAccess.CacheUserHistory;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MTGAHelper.Server.DataAccess
{
    public class UserMtgaDeckRepository
    {
        private readonly UserHistoryRepositoryGeneric<List<ConfigModelRawDeck>> cacheUserAllMtgaDecks;

        public UserMtgaDeckRepository(UserHistoryRepositoryGeneric<List<ConfigModelRawDeck>> cacheUserAllMtgaDecks)
        {
            this.cacheUserAllMtgaDecks = cacheUserAllMtgaDecks.Init(InfoByDateKeyEnum.Decks.ToString());
        }

        public async Task<List<ConfigModelRawDeck>> Get(string userId)
        {
            var decks = await cacheUserAllMtgaDecks.GetData(userId);

            var decksToReformat = decks.Where(i => i.Cards == null);
            if (decksToReformat.Any())
            {
                foreach (var d in decksToReformat)
                {
                    var cardsMain = d.CardsMain.Select(i => new DeckCardRaw
                    {
                        GrpId = i.Key,
                        Amount = i.Value,
                        Zone = DeckCardZoneEnum.Deck
                    });

                    var cardsSideboard = d.CardsSideboard.Select(i => new DeckCardRaw
                    {
                        GrpId = i.Key,
                        Amount = i.Value,
                        Zone = DeckCardZoneEnum.Sideboard,
                    });

                    // Migrate to new model
                    d.Cards = cardsMain.Union(cardsSideboard).ToArray();
                }

                await Save(userId, decks);
            }

            return decks;
        }

        public async Task Save(string userId, List<ConfigModelRawDeck> newData)
        {
            await cacheUserAllMtgaDecks.SaveToDisk(userId, newData);
        }
    }
}