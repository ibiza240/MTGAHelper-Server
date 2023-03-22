using MTGAHelper.Entity;
using MTGAHelper.Entity.MtgaDeckStats;
using MTGAHelper.Entity.MtgaOutputLog;
using MTGAHelper.Lib.CardProviders;
using MTGAHelper.Server.DataAccess;
using MTGAHelper.Server.DataAccess.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MTGAHelper.Lib.MtgaDeckStats
{
    public class MtgaDeckAnalysisBuilder
    {
        //MatchesCacheManager matchesCacheManager;
        readonly IReadOnlyDictionary<int, Card> dictAllCards;
        readonly IQueryHandler<MatchesWithDeckQuery, IEnumerable<MatchResult>> qMatchesWithDeck;
        readonly UserMtgaDeckRepository userDeckRepository;
        readonly UtilColors utilColors;

        public MtgaDeckAnalysisBuilder(
            //MatchesCacheManager matchesCacheManager, 
            IQueryHandler<MatchesWithDeckQuery, IEnumerable<MatchResult>> qMatchesWithDeck,
            ICardRepository cardRepo,
            UserMtgaDeckRepository userDeckRepository,
            UtilColors utilColors
            )
        {
            //this.matchesCacheManager = matchesCacheManager;
            dictAllCards = cardRepo;
            this.qMatchesWithDeck = qMatchesWithDeck;
            this.userDeckRepository = userDeckRepository;
            this.utilColors = utilColors;
        }

        public async Task<MtgaDeckAnalysis> GetDetail(string userId, string deckId)
        {
            var mtgaDecks = await userDeckRepository.Get(userId);
            var mtgaDeck = mtgaDecks?.FirstOrDefault(i => i.Id == deckId);
            if (mtgaDeck == null)
                return new MtgaDeckAnalysis
                {
                    DeckId = deckId
                };

            var matches = await qMatchesWithDeck.Handle(new MatchesWithDeckQuery(userId, deckId, new DateTime(2019,9,1)));

            var ret = new MtgaDeckAnalysis
            {
                DeckId = deckId,
                DeckName = mtgaDeck.Name,
                DeckImage = dictAllCards[mtgaDeck.DeckTileId].ImageArtUrl,
                MatchesInfo = matches
                    .Select(i =>
                    {
                        var firstGame = i.Games.FirstOrDefault();
                        return new MtgaDeckAnalysisMatchInfo
                        {
                            EventName = i.EventName,
                            FirstTurn = firstGame?.FirstTurn ?? FirstTurnEnum.Unknown,
                            Mulligans = i.Games.Sum(x => x.MulliganCount),
                            OpponentMulligans = i.Games.Sum(x => x.MulliganCountOpponent),
                            OpponentColors = utilColors.FromGrpIds(i.GetOpponentCardsSeen()),
                            OpponentRank = i.Opponent.RankingClass,
                            Outcome = i.Outcome,
                            StartDateTime = i.StartDateTime,

                        };
                    })
                    .ToArray(),
            };

            return ret;
        }
    }
}
