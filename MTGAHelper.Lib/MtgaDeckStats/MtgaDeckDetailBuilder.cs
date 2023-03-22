using MTGAHelper.Entity;
using MTGAHelper.Entity.MtgaDeckStats;
using MTGAHelper.Entity.MtgaOutputLog;
using MTGAHelper.Lib.CardProviders;
using MTGAHelper.Lib.MasteryPass;
using MTGAHelper.Server.DataAccess;
using MTGAHelper.Server.DataAccess.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MTGAHelper.Lib.MtgaDeckStats
{
    public class MtgaDeckDetailBuilder : MTGADeckBuilderBase
    {
        //MatchesCacheManager matchesCacheManager;
        private readonly IQueryHandler<MatchesWithDeckQuery, IEnumerable<MatchResult>> qMatchesWithDeck;

        private readonly UserMtgaDeckRepository cacheUserAllMtgaDecks;
        private readonly UtilColors utilColors;

        public MtgaDeckDetailBuilder(
            ICardRepository cardRepo,
            //MatchesCacheManager matchesCacheManager,
            IQueryHandler<MatchesWithDeckQuery, IEnumerable<MatchResult>> qMatchesWithDeck,
            UserMtgaDeckRepository cacheUserAllMtgaDecks,
            UtilColors utilColors)
            : base(cardRepo)
        {
            //this.matchesCacheManager = matchesCacheManager;
            this.qMatchesWithDeck = qMatchesWithDeck;
            this.cacheUserAllMtgaDecks = cacheUserAllMtgaDecks;
            this.utilColors = utilColors;
        }

        public async Task<MtgaDeckDetail> GetDetail(string userId, string deckId, string period)
        {
            var currentSet = "ONE";
            var lastSet = "BRO";
            var minDate = period == "currentset" ? SetStartingDates.DictStartingDate[currentSet] :
                period == "currentandpreviousset" ? SetStartingDates.DictStartingDate[lastSet] : new DateTime(2019, 9, 26);

            var matches = (await qMatchesWithDeck.Handle(new MatchesWithDeckQuery(userId, deckId, minDate)))
                .OrderByDescending(i => i.StartDateTime)
                //.Where(i => i.EventName != default)
                .ToArray();

            var mtgaDecks = await cacheUserAllMtgaDecks.Get(userId);
            var deck = mtgaDecks?.FirstOrDefault(i => i.Id == deckId);
            var (_, deckName, deckImage, mtgaDeck) = GetDeck(matches, deck);

            //if (deckName.StartsWith("Suicide")) System.Diagnostics.Debugger.Break();

            var matchesByFormat = matches
                .GroupBy(x => x.EventName ?? "Unknown")
                .ToDictionary(i => i.Key, i => i);

            //var stats = matchesByFormat
            //    .Select(i => new MtgaDeckStatsByFormat
            //    {
            //        Format = i.Key,
            //        NbWins = i.Value.Count(x => x.Outcome == GameOutcomeEnum.Victory),
            //        NbLosses = i.Value.Count(x => x.Outcome == GameOutcomeEnum.Defeat),
            //    })
            //    .ToArray();

            ////var deckUsed = new ConfigModelRawDeck();
            ////if (mtgaDeck != null)
            ////    deckUsed = Mapper.Map<ConfigModelRawDeck>(mtgaDeck);
            ////else if (matches.Any())
            ////    deckUsed = matches.Last().DeckUsed;

            var ret = new MtgaDeckDetail
            {
                DeckId = deckId,
                DeckName = deckName,
                DeckImage = deckImage,
                DeckColor = utilColors.FromGrpIds(mtgaDeck.Cards.Select(i => i.GrpId).ToArray()),
                CardsMain = mtgaDeck?.CardsMainWithCommander,
                CardsNotMainByZone = mtgaDeck.CardsNotMainByZone,
                ////DeckUsed = matches.Last().DeckUsed,
                ////DeckUsed = deckUsed,
                ////FirstPlayed = matches.Min(x => x.StartDateTime),
                ////LastPlayed = matches.Max(x => x.StartDateTime),
                //StatsByFormat = stats,
                Matches = matches,
            };

            return ret;
        }
    }
}